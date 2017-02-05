/**
 * Copyright (c) 2016-present, Przemyslaw Skibinski, Facebook, Inc.
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the
 * LICENSE file in the root directory of this source tree. An additional grant
 * of patent rights can be found in the PATENTS file in the same directory.
 */


#include <stdio.h>                 /* vsprintf */
#include <stdarg.h>                /* va_list, for z_gzprintf */
#include <zlib.h>
#include "zstd_zlibwrapper.h"
#define ZSTD_STATIC_LINKING_ONLY   /* ZSTD_MAGICNUMBER */
#include "zstd.h"
#define ZBUFF_STATIC_LINKING_ONLY  /* ZBUFF_createCCtx_advanced */
#include "zbuff.h"
#include "zstd_internal.h"         /* defaultCustomMem */


#define Z_INFLATE_SYNC              8
#define ZWRAP_HEADERSIZE            4
#define ZWRAP_DEFAULT_CLEVEL        5   /* Z_DEFAULT_COMPRESSION is translated to ZWRAP_DEFAULT_CLEVEL for zstd */

#define LOG_WRAPPER(...)  /* printf(__VA_ARGS__) */


#define FINISH_WITH_GZ_ERR(msg) { \
    (void)msg; \
    return Z_MEM_ERROR; \
}

#define FINISH_WITH_ERR(strm, message) { \
    strm->msg = message; \
    return Z_MEM_ERROR; \
}

#define FINISH_WITH_NULL_ERR(msg) { \
    (void)msg; \
    return NULL; \
}

#ifndef ZWRAP_USE_ZSTD
    #define ZWRAP_USE_ZSTD 0
#endif

static int g_useZSTD = ZWRAP_USE_ZSTD;   /* 0 = don't use ZSTD */



void useZSTD(int turn_on) { g_useZSTD = turn_on; }

int isUsingZSTD(void) { return g_useZSTD; }

const char * zstdVersion(void) { return ZSTD_VERSION_STRING; }

ZEXTERN const char * ZEXPORT z_zlibVersion OF((void)) { return zlibVersion();  }


static void* ZWRAP_allocFunction(void* opaque, size_t size)
{
    z_streamp strm = (z_streamp) opaque;
    void* address = strm->zalloc(strm->opaque, 1, size);
  /*  printf("ZWRAP alloc %p, %d \n", address, (int)size); */
    return address;
}

static void ZWRAP_freeFunction(void* opaque, void* address)
{
    z_streamp strm = (z_streamp) opaque;
    strm->zfree(strm->opaque, address);
   /* if (address) printf("ZWRAP free %p \n", address); */
}



/* *** Compression *** */

typedef struct {
    ZBUFF_CCtx* zbc;
    size_t bytesLeft;
    int compressionLevel;
    ZSTD_customMem customMem;
    z_stream allocFunc; /* copy of zalloc, zfree, opaque */
} ZWRAP_CCtx;


size_t ZWRAP_freeCCtx(ZWRAP_CCtx* zwc)
{
    if (zwc==NULL) return 0;   /* support free on NULL */
    ZBUFF_freeCCtx(zwc->zbc);
    zwc->customMem.customFree(zwc->customMem.opaque, zwc);
    return 0;
}


ZWRAP_CCtx* ZWRAP_createCCtx(z_streamp strm)
{
    ZWRAP_CCtx* zwc;

    if (strm->zalloc && strm->zfree) {
        zwc = (ZWRAP_CCtx*)strm->zalloc(strm->opaque, 1, sizeof(ZWRAP_CCtx));
        if (zwc==NULL) return NULL;
        memset(zwc, 0, sizeof(ZWRAP_CCtx));
        memcpy(&zwc->allocFunc, strm, sizeof(z_stream));
        { ZSTD_customMem ZWRAP_customMem = { ZWRAP_allocFunction, ZWRAP_freeFunction, &zwc->allocFunc };
          memcpy(&zwc->customMem, &ZWRAP_customMem, sizeof(ZSTD_customMem));
        }
    } else {
        zwc = (ZWRAP_CCtx*)defaultCustomMem.customAlloc(defaultCustomMem.opaque, sizeof(ZWRAP_CCtx));
        if (zwc==NULL) return NULL;
        memset(zwc, 0, sizeof(ZWRAP_CCtx));
        memcpy(&zwc->customMem, &defaultCustomMem, sizeof(ZSTD_customMem));
    }

    zwc->zbc = ZBUFF_createCCtx_advanced(zwc->customMem);
    if (zwc->zbc == NULL) { ZWRAP_freeCCtx(zwc); return NULL; }
    return zwc;
}


ZEXTERN int ZEXPORT z_deflateInit_ OF((z_streamp strm, int level,
                                     const char *version, int stream_size))
{
    ZWRAP_CCtx* zwc;

    if (!g_useZSTD) {
        LOG_WRAPPER("- deflateInit level=%d\n", level);
        return deflateInit_((strm), (level), version, stream_size);
    }

    LOG_WRAPPER("- deflateInit level=%d\n", level);
    zwc = ZWRAP_createCCtx(strm);
    if (zwc == NULL) return Z_MEM_ERROR;

    if (level == Z_DEFAULT_COMPRESSION)
        level = ZWRAP_DEFAULT_CLEVEL;

    { size_t const errorCode = ZBUFF_compressInit(zwc->zbc, level);
      if (ZSTD_isError(errorCode)) return Z_MEM_ERROR; }

    zwc->compressionLevel = level;
    strm->state = (struct internal_state*) zwc; /* use state which in not used by user */
    strm->total_in = 0;
    strm->total_out = 0;
    return Z_OK;
}


ZEXTERN int ZEXPORT z_deflateInit2_ OF((z_streamp strm, int level, int method,
                                      int windowBits, int memLevel,
                                      int strategy, const char *version,
                                      int stream_size))
{
    if (!g_useZSTD)
        return deflateInit2_(strm, level, method, windowBits, memLevel, strategy, version, stream_size);

    return z_deflateInit_ (strm, level, version, stream_size);
}


ZEXTERN int ZEXPORT z_deflateSetDictionary OF((z_streamp strm,
                                             const Bytef *dictionary,
                                             uInt  dictLength))
{
    if (!g_useZSTD)
        return deflateSetDictionary(strm, dictionary, dictLength);

    {   ZWRAP_CCtx* zwc = (ZWRAP_CCtx*) strm->state;
        LOG_WRAPPER("- deflateSetDictionary level=%d\n", (int)strm->data_type);
        { size_t const errorCode = ZBUFF_compressInitDictionary(zwc->zbc, dictionary, dictLength, zwc->compressionLevel);
          if (ZSTD_isError(errorCode)) return Z_MEM_ERROR; }
    }

    return Z_OK;
}


ZEXTERN int ZEXPORT z_deflate OF((z_streamp strm, int flush))
{
    ZWRAP_CCtx* zwc;

    if (!g_useZSTD) {
        int res = deflate(strm, flush);
        LOG_WRAPPER("- avail_in=%d total_in=%d total_out=%d\n", (int)strm->avail_in, (int)strm->total_in, (int)strm->total_out);
        return res;
    }

    zwc = (ZWRAP_CCtx*) strm->state;

    LOG_WRAPPER("deflate flush=%d avail_in=%d avail_out=%d total_in=%d total_out=%d\n", (int)flush, (int)strm->avail_in, (int)strm->avail_out, (int)strm->total_in, (int)strm->total_out);
    if (strm->avail_in > 0) {
        size_t dstCapacity = strm->avail_out;
        size_t srcSize = strm->avail_in;
        size_t const errorCode = ZBUFF_compressContinue(zwc->zbc, strm->next_out, &dstCapacity, strm->next_in, &srcSize);
        LOG_WRAPPER("ZBUFF_compressContinue srcSize=%d dstCapacity=%d\n", (int)srcSize, (int)dstCapacity);
        if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
        strm->next_out += dstCapacity;
        strm->total_out += dstCapacity;
        strm->avail_out -= dstCapacity;
        strm->total_in += srcSize;
        strm->next_in += srcSize;
        strm->avail_in -= srcSize;
    }

    if (flush == Z_FULL_FLUSH) FINISH_WITH_ERR(strm, "Z_FULL_FLUSH is not supported!");

    if (flush == Z_FINISH) {
        size_t bytesLeft;
        size_t dstCapacity = strm->avail_out;
        if (zwc->bytesLeft) {
            bytesLeft = ZBUFF_compressFlush(zwc->zbc, strm->next_out, &dstCapacity);
            LOG_WRAPPER("ZBUFF_compressFlush avail_out=%d dstCapacity=%d bytesLeft=%d\n", (int)strm->avail_out, (int)dstCapacity, (int)bytesLeft);
        } else {
            bytesLeft = ZBUFF_compressEnd(zwc->zbc, strm->next_out, &dstCapacity);
            LOG_WRAPPER("ZBUFF_compressEnd dstCapacity=%d bytesLeft=%d\n", (int)dstCapacity, (int)bytesLeft);
        }
        if (ZSTD_isError(bytesLeft)) return Z_MEM_ERROR;
        strm->next_out += dstCapacity;
        strm->total_out += dstCapacity;
        strm->avail_out -= dstCapacity;
        if (flush == Z_FINISH && bytesLeft == 0) return Z_STREAM_END;
        zwc->bytesLeft = bytesLeft;
    }

    if (flush == Z_SYNC_FLUSH) {
        size_t bytesLeft;
        size_t dstCapacity = strm->avail_out;
        bytesLeft = ZBUFF_compressFlush(zwc->zbc, strm->next_out, &dstCapacity);
        LOG_WRAPPER("ZBUFF_compressFlush avail_out=%d dstCapacity=%d bytesLeft=%d\n", (int)strm->avail_out, (int)dstCapacity, (int)bytesLeft);
        if (ZSTD_isError(bytesLeft)) return Z_MEM_ERROR;
        strm->next_out += dstCapacity;
        strm->total_out += dstCapacity;
        strm->avail_out -= dstCapacity;
        zwc->bytesLeft = bytesLeft;
    }
    return Z_OK;
}


ZEXTERN int ZEXPORT z_deflateEnd OF((z_streamp strm))
{
    if (!g_useZSTD) {
        LOG_WRAPPER("- deflateEnd\n");
        return deflateEnd(strm);
    }
    LOG_WRAPPER("- deflateEnd total_in=%d total_out=%d\n", (int)(strm->total_in), (int)(strm->total_out));
    {   ZWRAP_CCtx* zwc = (ZWRAP_CCtx*) strm->state;
        size_t const errorCode = ZWRAP_freeCCtx(zwc);
        if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
    }
    return Z_OK;
}


ZEXTERN uLong ZEXPORT z_deflateBound OF((z_streamp strm,
                                       uLong sourceLen))
{
    if (!g_useZSTD)
        return deflateBound(strm, sourceLen);

    return ZSTD_compressBound(sourceLen);
}


ZEXTERN int ZEXPORT z_deflateParams OF((z_streamp strm,
                                      int level,
                                      int strategy))
{
    if (!g_useZSTD) {
        LOG_WRAPPER("- deflateParams level=%d strategy=%d\n", level, strategy);
        return deflateParams(strm, level, strategy);
    }

    return Z_OK;
}





/* *** Decompression *** */

typedef struct {
    ZBUFF_DCtx* zbd;
    char headerBuf[ZWRAP_HEADERSIZE];
    int errorCount;

    /* zlib params */
    int stream_size;
    char *version;
    int windowBits;
    ZSTD_customMem customMem;
    z_stream allocFunc; /* copy of zalloc, zfree, opaque */
} ZWRAP_DCtx;


ZWRAP_DCtx* ZWRAP_createDCtx(z_streamp strm)
{
    ZWRAP_DCtx* zwd;

    if (strm->zalloc && strm->zfree) {
        zwd = (ZWRAP_DCtx*)strm->zalloc(strm->opaque, 1, sizeof(ZWRAP_DCtx));
        if (zwd==NULL) return NULL;
        memset(zwd, 0, sizeof(ZWRAP_DCtx));
        memcpy(&zwd->allocFunc, strm, sizeof(z_stream));
        { ZSTD_customMem ZWRAP_customMem = { ZWRAP_allocFunction, ZWRAP_freeFunction, &zwd->allocFunc };
          memcpy(&zwd->customMem, &ZWRAP_customMem, sizeof(ZSTD_customMem));
        }
    } else {
        zwd = (ZWRAP_DCtx*)defaultCustomMem.customAlloc(defaultCustomMem.opaque, sizeof(ZWRAP_DCtx));
        if (zwd==NULL) return NULL;
        memset(zwd, 0, sizeof(ZWRAP_DCtx));
        memcpy(&zwd->customMem, &defaultCustomMem, sizeof(ZSTD_customMem));
    }

    return zwd;
}


size_t ZWRAP_freeDCtx(ZWRAP_DCtx* zwd)
{
    if (zwd==NULL) return 0;   /* support free on null */
    ZBUFF_freeDCtx(zwd->zbd);
    if (zwd->version) zwd->customMem.customFree(zwd->customMem.opaque, zwd->version);
    zwd->customMem.customFree(zwd->customMem.opaque, zwd);
    return 0;
}


ZEXTERN int ZEXPORT z_inflateInit_ OF((z_streamp strm,
                                     const char *version, int stream_size))
{
    ZWRAP_DCtx* zwd = ZWRAP_createDCtx(strm);
    LOG_WRAPPER("- inflateInit\n");
    if (zwd == NULL) { strm->state = NULL; return Z_MEM_ERROR; }

    zwd->version = zwd->customMem.customAlloc(zwd->customMem.opaque, strlen(version) + 1);
    if (zwd->version == NULL) { ZWRAP_freeDCtx(zwd); strm->state = NULL; return Z_MEM_ERROR; }
    strcpy(zwd->version, version);

    zwd->stream_size = stream_size;
    strm->state = (struct internal_state*) zwd; /* use state which in not used by user */
    strm->total_in = 0;
    strm->total_out = 0;
    strm->reserved = 1; /* mark as unknown steam */

    return Z_OK;
}


ZEXTERN int ZEXPORT z_inflateInit2_ OF((z_streamp strm, int  windowBits,
                                      const char *version, int stream_size))
{
    int ret = z_inflateInit_ (strm, version, stream_size);
    if (ret == Z_OK) {
        ZWRAP_DCtx* zwd = (ZWRAP_DCtx*)strm->state;
        zwd->windowBits = windowBits;
    }
    return ret;
}


ZEXTERN int ZEXPORT z_inflateSetDictionary OF((z_streamp strm,
                                             const Bytef *dictionary,
                                             uInt  dictLength))
{
    if (!strm->reserved)
        return inflateSetDictionary(strm, dictionary, dictLength);

    LOG_WRAPPER("- inflateSetDictionary\n");
    {   size_t errorCode;
        ZWRAP_DCtx* zwd = (ZWRAP_DCtx*) strm->state;
        if (strm->state == NULL) return Z_MEM_ERROR;
        errorCode = ZBUFF_decompressInitDictionary(zwd->zbd, dictionary, dictLength);
        if (ZSTD_isError(errorCode)) { ZWRAP_freeDCtx(zwd); strm->state = NULL; return Z_MEM_ERROR; }

        if (strm->total_in == ZSTD_frameHeaderSize_min) {
            size_t dstCapacity = 0;
            size_t srcSize = strm->total_in;
            errorCode = ZBUFF_decompressContinue(zwd->zbd, strm->next_out, &dstCapacity, zwd->headerBuf, &srcSize);
            LOG_WRAPPER("ZBUFF_decompressContinue3 errorCode=%d srcSize=%d dstCapacity=%d\n", (int)errorCode, (int)srcSize, (int)dstCapacity);
            if (dstCapacity > 0 || ZSTD_isError(errorCode)) {
                LOG_WRAPPER("ERROR: ZBUFF_decompressContinue %s\n", ZSTD_getErrorName(errorCode));
                ZWRAP_freeDCtx(zwd); strm->state = NULL;
                return Z_MEM_ERROR;
            }
        }
    }

    return Z_OK;
}


ZEXTERN int ZEXPORT z_inflate OF((z_streamp strm, int flush))
{
    if (!strm->reserved)
        return inflate(strm, flush);

    if (strm->avail_in > 0) {
        size_t errorCode, dstCapacity, srcSize;
        ZWRAP_DCtx* zwd = (ZWRAP_DCtx*) strm->state;
        if (strm->state == NULL) return Z_MEM_ERROR;
        LOG_WRAPPER("inflate avail_in=%d avail_out=%d total_in=%d total_out=%d\n", (int)strm->avail_in, (int)strm->avail_out, (int)strm->total_in, (int)strm->total_out);
        if (strm->total_in < ZWRAP_HEADERSIZE)
        {
            srcSize = MIN(strm->avail_in, ZWRAP_HEADERSIZE - strm->total_in);
            memcpy(zwd->headerBuf+strm->total_in, strm->next_in, srcSize);
            strm->total_in += srcSize;
            strm->next_in += srcSize;
            strm->avail_in -= srcSize;
            if (strm->total_in < ZWRAP_HEADERSIZE) return Z_OK;

            if (MEM_readLE32(zwd->headerBuf) != ZSTD_MAGICNUMBER) {
                z_stream strm2;
                strm2.next_in = strm->next_in;
                strm2.avail_in = strm->avail_in;
                strm2.next_out = strm->next_out;
                strm2.avail_out = strm->avail_out;

                if (zwd->windowBits)
                    errorCode = inflateInit2_(strm, zwd->windowBits, zwd->version, zwd->stream_size);
                else
                    errorCode = inflateInit_(strm, zwd->version, zwd->stream_size);
                LOG_WRAPPER("ZLIB inflateInit errorCode=%d\n", (int)errorCode);
                if (errorCode != Z_OK) { ZWRAP_freeDCtx(zwd); strm->state = NULL; return errorCode; }

                /* inflate header */
                strm->next_in = (unsigned char*)zwd->headerBuf;
                strm->avail_in = ZWRAP_HEADERSIZE;
                strm->avail_out = 0;
                errorCode = inflate(strm, Z_NO_FLUSH);
                LOG_WRAPPER("ZLIB inflate errorCode=%d strm->avail_in=%d\n", (int)errorCode, (int)strm->avail_in);
                if (errorCode != Z_OK) { ZWRAP_freeDCtx(zwd); strm->state = NULL; return errorCode; }
                if (strm->avail_in > 0) goto error;

                strm->next_in = strm2.next_in;
                strm->avail_in = strm2.avail_in;
                strm->next_out = strm2.next_out;
                strm->avail_out = strm2.avail_out;

                strm->reserved = 0; /* mark as zlib stream */
                errorCode = ZWRAP_freeDCtx(zwd);
                if (ZSTD_isError(errorCode)) goto error;

                if (flush == Z_INFLATE_SYNC) return inflateSync(strm);
                return inflate(strm, flush);
            }

            zwd->zbd = ZBUFF_createDCtx_advanced(zwd->customMem);
            if (zwd->zbd == NULL) goto error;

            errorCode = ZBUFF_decompressInit(zwd->zbd);
            if (ZSTD_isError(errorCode)) goto error;

            srcSize = ZWRAP_HEADERSIZE;
            dstCapacity = 0;
            errorCode = ZBUFF_decompressContinue(zwd->zbd, strm->next_out, &dstCapacity, zwd->headerBuf, &srcSize);
            LOG_WRAPPER("ZBUFF_decompressContinue1 errorCode=%d srcSize=%d dstCapacity=%d\n", (int)errorCode, (int)srcSize, (int)dstCapacity);
            if (ZSTD_isError(errorCode)) {
                LOG_WRAPPER("ERROR: ZBUFF_decompressContinue %s\n", ZSTD_getErrorName(errorCode));
                goto error;
            }
            if (strm->avail_in == 0) return Z_OK;
        }

        srcSize = strm->avail_in;
        dstCapacity = strm->avail_out;
        errorCode = ZBUFF_decompressContinue(zwd->zbd, strm->next_out, &dstCapacity, strm->next_in, &srcSize);
        LOG_WRAPPER("ZBUFF_decompressContinue2 errorCode=%d srcSize=%d dstCapacity=%d\n", (int)errorCode, (int)srcSize, (int)dstCapacity);
        if (ZSTD_isError(errorCode)) {
            LOG_WRAPPER("ERROR: ZBUFF_decompressContinue %s\n", ZSTD_getErrorName(errorCode));
            zwd->errorCount++;
            if (zwd->errorCount<=1) return Z_NEED_DICT; else goto error;
        }
        strm->next_out += dstCapacity;
        strm->total_out += dstCapacity;
        strm->avail_out -= dstCapacity;
        strm->total_in += srcSize;
        strm->next_in += srcSize;
        strm->avail_in -= srcSize;
        if (errorCode == 0) return Z_STREAM_END;
        return Z_OK;
error:
        ZWRAP_freeDCtx(zwd);
        strm->state = NULL;
        return Z_MEM_ERROR;
    }
    return Z_OK;
}


ZEXTERN int ZEXPORT z_inflateEnd OF((z_streamp strm))
{
    int ret = Z_OK;
    if (!strm->reserved)
        return inflateEnd(strm);

    LOG_WRAPPER("- inflateEnd total_in=%d total_out=%d\n", (int)(strm->total_in), (int)(strm->total_out));
    {   ZWRAP_DCtx* zwd = (ZWRAP_DCtx*) strm->state;
        size_t const errorCode = ZWRAP_freeDCtx(zwd);
        strm->state = NULL;
        if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
    }
    return ret;
}


ZEXTERN int ZEXPORT z_inflateSync OF((z_streamp strm))
{
    return z_inflate(strm, Z_INFLATE_SYNC);
}




/* Advanced compression functions */
ZEXTERN int ZEXPORT z_deflateCopy OF((z_streamp dest,
                                    z_streamp source))
{
    if (!g_useZSTD)
        return deflateCopy(dest, source);
    FINISH_WITH_ERR(source, "deflateCopy is not supported!");
}


ZEXTERN int ZEXPORT z_deflateReset OF((z_streamp strm))
{
    if (!g_useZSTD)
        return deflateReset(strm);
    FINISH_WITH_ERR(strm, "deflateReset is not supported!");
}


ZEXTERN int ZEXPORT z_deflateTune OF((z_streamp strm,
                                    int good_length,
                                    int max_lazy,
                                    int nice_length,
                                    int max_chain))
{
    if (!g_useZSTD)
        return deflateTune(strm, good_length, max_lazy, nice_length, max_chain);
    FINISH_WITH_ERR(strm, "deflateTune is not supported!");
}


#if ZLIB_VERNUM >= 0x1260
ZEXTERN int ZEXPORT z_deflatePending OF((z_streamp strm,
                                       unsigned *pending,
                                       int *bits))
{
    if (!g_useZSTD)
        return deflatePending(strm, pending, bits);
    FINISH_WITH_ERR(strm, "deflatePending is not supported!");
}
#endif


ZEXTERN int ZEXPORT z_deflatePrime OF((z_streamp strm,
                                     int bits,
                                     int value))
{
    if (!g_useZSTD)
        return deflatePrime(strm, bits, value);
    FINISH_WITH_ERR(strm, "deflatePrime is not supported!");
}


ZEXTERN int ZEXPORT z_deflateSetHeader OF((z_streamp strm,
                                         gz_headerp head))
{
    if (!g_useZSTD)
        return deflateSetHeader(strm, head);
    FINISH_WITH_ERR(strm, "deflateSetHeader is not supported!");
}




/* Advanced compression functions */
#if ZLIB_VERNUM >= 0x1280
ZEXTERN int ZEXPORT z_inflateGetDictionary OF((z_streamp strm,
                                             Bytef *dictionary,
                                             uInt  *dictLength))
{
    if (!strm->reserved)
        return inflateGetDictionary(strm, dictionary, dictLength);
    FINISH_WITH_ERR(strm, "inflateGetDictionary is not supported!");
}
#endif


ZEXTERN int ZEXPORT z_inflateCopy OF((z_streamp dest,
                                    z_streamp source))
{
    if (!g_useZSTD)
        return inflateCopy(dest, source);
    FINISH_WITH_ERR(source, "inflateCopy is not supported!");
}


ZEXTERN int ZEXPORT z_inflateReset OF((z_streamp strm))
{
    if (!strm->reserved)
        return inflateReset(strm);
    FINISH_WITH_ERR(strm, "inflateReset is not supported!");
}


#if ZLIB_VERNUM >= 0x1240
ZEXTERN int ZEXPORT z_inflateReset2 OF((z_streamp strm,
                                      int windowBits))
{
    if (!strm->reserved)
        return inflateReset2(strm, windowBits);
    FINISH_WITH_ERR(strm, "inflateReset2 is not supported!");
}
#endif


#if ZLIB_VERNUM >= 0x1240
ZEXTERN long ZEXPORT z_inflateMark OF((z_streamp strm))
{
    if (!strm->reserved)
        return inflateMark(strm);
    FINISH_WITH_ERR(strm, "inflateMark is not supported!");
}
#endif


ZEXTERN int ZEXPORT z_inflatePrime OF((z_streamp strm,
                                     int bits,
                                     int value))
{
    if (!strm->reserved)
        return inflatePrime(strm, bits, value);
    FINISH_WITH_ERR(strm, "inflatePrime is not supported!");
}


ZEXTERN int ZEXPORT z_inflateGetHeader OF((z_streamp strm,
                                         gz_headerp head))
{
    if (!strm->reserved)
        return inflateGetHeader(strm, head);
    FINISH_WITH_ERR(strm, "inflateGetHeader is not supported!");
}


ZEXTERN int ZEXPORT z_inflateBackInit_ OF((z_streamp strm, int windowBits,
                                         unsigned char FAR *window,
                                         const char *version,
                                         int stream_size))
{
    if (!strm->reserved)
        return inflateBackInit_(strm, windowBits, window, version, stream_size);
    FINISH_WITH_ERR(strm, "inflateBackInit is not supported!");
}


ZEXTERN int ZEXPORT z_inflateBack OF((z_streamp strm,
                                    in_func in, void FAR *in_desc,
                                    out_func out, void FAR *out_desc))
{
    if (!strm->reserved)
        return inflateBack(strm, in, in_desc, out, out_desc);
    FINISH_WITH_ERR(strm, "inflateBack is not supported!");
}


ZEXTERN int ZEXPORT z_inflateBackEnd OF((z_streamp strm))
{
    if (!strm->reserved)
        return inflateBackEnd(strm);
    FINISH_WITH_ERR(strm, "inflateBackEnd is not supported!");
}


ZEXTERN uLong ZEXPORT z_zlibCompileFlags OF((void)) { return zlibCompileFlags(); };



                        /* utility functions */
#ifndef Z_SOLO

ZEXTERN int ZEXPORT z_compress OF((Bytef *dest,   uLongf *destLen,
                                 const Bytef *source, uLong sourceLen))
{
    if (!g_useZSTD)
        return compress(dest, destLen, source, sourceLen);

    { size_t dstCapacity = *destLen;
      size_t const errorCode = ZSTD_compress(dest, dstCapacity, source, sourceLen, ZWRAP_DEFAULT_CLEVEL);
      LOG_WRAPPER("z_compress sourceLen=%d dstCapacity=%d\n", (int)sourceLen, (int)dstCapacity);
      if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
      *destLen = errorCode;
    }
    return Z_OK;
}


ZEXTERN int ZEXPORT z_compress2 OF((Bytef *dest,   uLongf *destLen,
                                  const Bytef *source, uLong sourceLen,
                                  int level))
{
    if (!g_useZSTD)
        return compress2(dest, destLen, source, sourceLen, level);

    { size_t dstCapacity = *destLen;
      size_t const errorCode = ZSTD_compress(dest, dstCapacity, source, sourceLen, level);
      if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
      *destLen = errorCode;
    }
    return Z_OK;
}


ZEXTERN uLong ZEXPORT z_compressBound OF((uLong sourceLen))
{
    if (!g_useZSTD)
        return compressBound(sourceLen);

    return ZSTD_compressBound(sourceLen);
}


ZEXTERN int ZEXPORT z_uncompress OF((Bytef *dest,   uLongf *destLen,
                                   const Bytef *source, uLong sourceLen))
{
    if (sourceLen < 4 || MEM_readLE32(source) != ZSTD_MAGICNUMBER)
        return uncompress(dest, destLen, source, sourceLen);

    { size_t dstCapacity = *destLen;
      size_t const errorCode = ZSTD_decompress(dest, dstCapacity, source, sourceLen);
      if (ZSTD_isError(errorCode)) return Z_MEM_ERROR;
      *destLen = errorCode;
     }
    return Z_OK;
}



                        /* gzip file access functions */
ZEXTERN gzFile ZEXPORT z_gzopen OF((const char *path, const char *mode))
{
    if (!g_useZSTD)
        return gzopen(path, mode);
    FINISH_WITH_NULL_ERR("gzopen is not supported!");
}


ZEXTERN gzFile ZEXPORT z_gzdopen OF((int fd, const char *mode))
{
    if (!g_useZSTD)
        return gzdopen(fd, mode);
    FINISH_WITH_NULL_ERR("gzdopen is not supported!");
}


#if ZLIB_VERNUM >= 0x1240
ZEXTERN int ZEXPORT z_gzbuffer OF((gzFile file, unsigned size))
{
    if (!g_useZSTD)
        return gzbuffer(file, size);
    FINISH_WITH_GZ_ERR("gzbuffer is not supported!");
}


ZEXTERN z_off_t ZEXPORT z_gzoffset OF((gzFile file))
{
    if (!g_useZSTD)
        return gzoffset(file);
    FINISH_WITH_GZ_ERR("gzoffset is not supported!");
}


ZEXTERN int ZEXPORT z_gzclose_r OF((gzFile file))
{
    if (!g_useZSTD)
        return gzclose_r(file);
    FINISH_WITH_GZ_ERR("gzclose_r is not supported!");
}


ZEXTERN int ZEXPORT z_gzclose_w OF((gzFile file))
{
    if (!g_useZSTD)
        return gzclose_w(file);
    FINISH_WITH_GZ_ERR("gzclose_w is not supported!");
}
#endif


ZEXTERN int ZEXPORT z_gzsetparams OF((gzFile file, int level, int strategy))
{
    if (!g_useZSTD)
        return gzsetparams(file, level, strategy);
    FINISH_WITH_GZ_ERR("gzsetparams is not supported!");
}


ZEXTERN int ZEXPORT z_gzread OF((gzFile file, voidp buf, unsigned len))
{
    if (!g_useZSTD)
        return gzread(file, buf, len);
    FINISH_WITH_GZ_ERR("gzread is not supported!");
}


ZEXTERN int ZEXPORT z_gzwrite OF((gzFile file,
                                voidpc buf, unsigned len))
{
    if (!g_useZSTD)
        return gzwrite(file, buf, len);
    FINISH_WITH_GZ_ERR("gzwrite is not supported!");
}


#if ZLIB_VERNUM >= 0x1260
ZEXTERN int ZEXPORTVA z_gzprintf Z_ARG((gzFile file, const char *format, ...))
#else
ZEXTERN int ZEXPORTVA z_gzprintf OF((gzFile file, const char *format, ...))
#endif
{
    if (!g_useZSTD) {
        int ret;
        char buf[1024];
        va_list args;
        va_start (args, format);
        ret = vsprintf (buf, format, args);
        va_end (args);

        ret = gzprintf(file, buf);
        return ret;
    }
    FINISH_WITH_GZ_ERR("gzprintf is not supported!");
}


ZEXTERN int ZEXPORT z_gzputs OF((gzFile file, const char *s))
{
    if (!g_useZSTD)
        return gzputs(file, s);
    FINISH_WITH_GZ_ERR("gzputs is not supported!");
}


ZEXTERN char * ZEXPORT z_gzgets OF((gzFile file, char *buf, int len))
{
    if (!g_useZSTD)
        return gzgets(file, buf, len);
    FINISH_WITH_NULL_ERR("gzgets is not supported!");
}


ZEXTERN int ZEXPORT z_gzputc OF((gzFile file, int c))
{
    if (!g_useZSTD)
        return gzputc(file, c);
    FINISH_WITH_GZ_ERR("gzputc is not supported!");
}


#if ZLIB_VERNUM == 0x1260
ZEXTERN int ZEXPORT z_gzgetc_ OF((gzFile file))
#else
ZEXTERN int ZEXPORT z_gzgetc OF((gzFile file))
#endif
{
    if (!g_useZSTD)
        return gzgetc(file);
    FINISH_WITH_GZ_ERR("gzgetc is not supported!");
}


ZEXTERN int ZEXPORT z_gzungetc OF((int c, gzFile file))
{
    if (!g_useZSTD)
        return gzungetc(c, file);
    FINISH_WITH_GZ_ERR("gzungetc is not supported!");
}


ZEXTERN int ZEXPORT z_gzflush OF((gzFile file, int flush))
{
    if (!g_useZSTD)
        return gzflush(file, flush);
    FINISH_WITH_GZ_ERR("gzflush is not supported!");
}


ZEXTERN z_off_t ZEXPORT z_gzseek OF((gzFile file, z_off_t offset, int whence))
{
    if (!g_useZSTD)
        return gzseek(file, offset, whence);
    FINISH_WITH_GZ_ERR("gzseek is not supported!");
}


ZEXTERN int ZEXPORT    z_gzrewind OF((gzFile file))
{
    if (!g_useZSTD)
        return gzrewind(file);
    FINISH_WITH_GZ_ERR("gzrewind is not supported!");
}


ZEXTERN z_off_t ZEXPORT    z_gztell OF((gzFile file))
{
    if (!g_useZSTD)
        return gztell(file);
    FINISH_WITH_GZ_ERR("gztell is not supported!");
}


ZEXTERN int ZEXPORT z_gzeof OF((gzFile file))
{
    if (!g_useZSTD)
        return gzeof(file);
    FINISH_WITH_GZ_ERR("gzeof is not supported!");
}


ZEXTERN int ZEXPORT z_gzdirect OF((gzFile file))
{
    if (!g_useZSTD)
        return gzdirect(file);
    FINISH_WITH_GZ_ERR("gzdirect is not supported!");
}


ZEXTERN int ZEXPORT    z_gzclose OF((gzFile file))
{
    if (!g_useZSTD)
        return gzclose(file);
    FINISH_WITH_GZ_ERR("gzclose is not supported!");
}


ZEXTERN const char * ZEXPORT z_gzerror OF((gzFile file, int *errnum))
{
    if (!g_useZSTD)
        return gzerror(file, errnum);
    FINISH_WITH_NULL_ERR("gzerror is not supported!");
}


ZEXTERN void ZEXPORT z_gzclearerr OF((gzFile file))
{
    if (!g_useZSTD)
        gzclearerr(file);
}


#endif /* !Z_SOLO */


                        /* checksum functions */

ZEXTERN uLong ZEXPORT z_adler32 OF((uLong adler, const Bytef *buf, uInt len))
{
    return adler32(adler, buf, len);
}

ZEXTERN uLong ZEXPORT z_crc32   OF((uLong crc, const Bytef *buf, uInt len))
{
    return crc32(crc, buf, len);
}
