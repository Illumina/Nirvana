#include "BlockGZipUtils.h"

// inflate the block in compressed_block into uncompressed_block
int uncompress_block(char* uncompressed_block, char* compressed_block, int block_length)
{
	z_stream zs;
	zs.zalloc    = NULL;
	zs.zfree     = NULL;
	zs.next_in   = (Bytef*)compressed_block + 18;
	zs.avail_in  = block_length - 16;
	zs.next_out  = (Bytef*)uncompressed_block;
	zs.avail_out = BGZF_MAX_BLOCK_SIZE;

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
