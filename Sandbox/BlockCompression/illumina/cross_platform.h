#pragma once

#ifndef CROSS_PLATFORM_H
#define CROSS_PLATFORM_H

#ifdef _MSC_VER

#include <intrin.h>

// ===============================
// Visual Studio-specific settings
// ===============================

#ifndef __cplusplus
#define inline __inline
#endif
#define __func__ __FUNCTION__

//#define PIPE_ERRNO EINVAL
#define R_OK 4

#define ftello(a)     _ftelli64(a)
#define fseeko(a,b,c) _fseeki64(a,b,c)

#define STDIN_FILENO _fileno(stdin)
#define STDOUT_FILENO _fileno(stdout)

#define u_int32_t uint32_t
#define ssize_t __int64

#if _MSC_VER < 1800
#define isnan _isnan
#endif

// POSIX deprecation
//#define access _access
#define alloca _alloca
#define close _close
//#define fdopen _fdopen
//#define fileno _fileno
//#define isatty _isatty
// // in VS2013
#define lseek _lseek
#define lseek64 _lseeki64
#define open _open
//#define read(fd, buf, count) _read(fd, buf, (unsigned int)count)
//#define setmode _setmode
//#define strcasecmp _strcmpi
//#define strdup _strdup // in VS2013
//#define strncasecmp _strnicmp
#define write _write
//#define strtoull _strtoui64

inline int __sync_fetch_and_add(int volatile* addr, int val)
{
	return (int)_InterlockedExchangeAdd64((__int64 volatile *)addr, val);
}

#else

// =====================
// gcc-specific settings
// =====================

//#define AttributeUsed __attribute((used))
//#define PIPE_ERRNO ESPIPE

#endif

#endif
