#include "BlockGZipUtils.h"

// inflate the block in compressed_block into uncompressed_block
int bgzf_decompress(char* destination, int destinationLen, char* source, int sourceLen)
{
	z_stream zs;
	zs.zalloc    = NULL;
	zs.zfree     = NULL;
	zs.next_in   = (Bytef*)source + 18;
	zs.avail_in  = sourceLen - 16;
	zs.next_out  = (Bytef*)destination;
	zs.avail_out = destinationLen;

	if (inflateInit2(&zs, -15) != Z_OK) return -1;

	if (inflate(&zs, Z_FINISH) != Z_STREAM_END) {
		inflateEnd(&zs);
		return -2;
	}

	if (inflateEnd(&zs) != Z_OK) return -3;

	return (int)zs.total_out;
}

// grab the zlib version
char* GetVersion() {
	return ZLIB_VERSION;
}
