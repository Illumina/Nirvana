#include "zlib.h"
#include <stdio.h>
#include <stdlib.h>
#include "bgzf.h"
#include <BlockGZipUtils.h>
#include "quicklz.h"

int main(int argc, char *argv[])
{
#ifdef _MSC_VER
	const char* input_path                = "E:\\Data\\Nirvana\\mother.json";
	const char* zlib_compressed_path      = "E:\\Data\\Nirvana\\mother.json.zlib_compressed";
	const char* zlib_uncompressed_path    = "E:\\Data\\Nirvana\\mother.json.zlib_uncompressed";
	const char* quicklz_compressed_path   = "E:\\Data\\Nirvana\\mother.json.quicklz_compressed";
	const char* quicklz_uncompressed_path = "E:\\Data\\Nirvana\\mother.json.quicklz_uncompressed";
#else
	const char* input_path                = "/home/nirvana/BlockCompression/bin/mother.json";
	const char* zlib_compressed_path      = "/home/nirvana/BlockCompression/bin/mother.json.zlib_compressed";
	const char* zlib_uncompressed_path    = "/home/nirvana/BlockCompression/bin/mother.json.zlib_uncompressed";
	const char* quicklz_compressed_path   = "/home/nirvana/BlockCompression/bin/mother.json.quicklz_compressed";
	const char* quicklz_uncompressed_path = "/home/nirvana/BlockCompression/bin/mother.json.quicklz_uncompressed";
#endif

#define DESTINATION_BLOCK_SIZE (BGZF_MAX_BLOCK_SIZE+400)

	char* compressed_block = malloc(DESTINATION_BLOCK_SIZE);
	char* uncompressed_block = malloc(BGZF_MAX_BLOCK_SIZE);

	// =====================
	// zlib compression pass
	// =====================

	printf("- staring zlib compression pass\n");

	FILE* in = fopen(input_path, "rb");
	FILE* out = fopen(zlib_compressed_path, "wb");

	if (!in) {
		printf("ERROR: Unable to open: %s\n", input_path);
		exit(1);
	}

	while (!feof(in))
	{
		int numBytesRead = (int)fread(uncompressed_block, 1, BGZF_MAX_BLOCK_SIZE, in);

		int comp_size = BGZF_MAX_BLOCK_SIZE;
		int ret = bgzf_compress(compressed_block, &comp_size, uncompressed_block, numBytesRead, 1);

		if (ret == -1) {
			printf("ERROR: could not execute bgzf_compress: %d\n", ret);
			exit(1);
		}

		fwrite((char*)&comp_size, 4, 1, out);
		fwrite(compressed_block, 1, comp_size, out);
	}

	fclose(out);
	fclose(in);

	// =======================
	// zlib decompression pass
	// =======================

	printf("- staring zlib decompression pass\n");

	in = fopen(zlib_compressed_path, "rb");
	out = fopen(zlib_uncompressed_path, "wb");

	while (!feof(in))
	{
		int comp_size;
		int numBytesRead = (int)fread((char*)&comp_size, 4, 1, in);
		if (numBytesRead == 0) break;

		numBytesRead = (int)fread(compressed_block, 1, comp_size, in);
		if (numBytesRead == 0) break;

		int uncompressed_size = uncompress_block(uncompressed_block, compressed_block, comp_size);
		fwrite(uncompressed_block, 1, uncompressed_size, out);
	}

	fclose(out);
	fclose(in);

	// ========================
	// quicklz compression pass
	// ========================

	printf("- staring quicklz compression pass\n");

	in = fopen(input_path, "rb");
	out = fopen(quicklz_compressed_path, "wb");

	while (!feof(in))
	{
		int numBytesRead = (int)fread(uncompressed_block, 1, BGZF_MAX_BLOCK_SIZE, in);
		int comp_size = QuickLzCompress(uncompressed_block, numBytesRead, compressed_block, DESTINATION_BLOCK_SIZE);

		fwrite((char*)&comp_size, 4, 1, out);
		fwrite(compressed_block, 1, comp_size, out);
	}

	fclose(out);
	fclose(in);

	// ==========================
	// quicklz decompression pass
	// ==========================

	printf("- staring quicklz decompression pass\n");

	in = fopen(quicklz_compressed_path, "rb");
	out = fopen(quicklz_uncompressed_path, "wb");

	while (!feof(in))
	{
		int comp_size;
		int numBytesRead = (int)fread((char*)&comp_size, 4, 1, in);
		if (numBytesRead == 0) break;

		numBytesRead = (int)fread(compressed_block, 1, comp_size, in);
		if (numBytesRead == 0) break;

		int uncompressed_size = QuickLzDecompress(compressed_block, uncompressed_block, BGZF_MAX_BLOCK_SIZE);
		fwrite(uncompressed_block, 1, uncompressed_size, out);
	}

	fclose(out);
	fclose(in);

	return 0;
}
