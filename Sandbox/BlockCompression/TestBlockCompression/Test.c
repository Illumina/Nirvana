#include "zlib.h"
#include <stdio.h>
#include <stdlib.h>
#include "bgzf.h"
#include <BlockGZipUtils.h>
#include "quicklz.h"
#include "Concat.h"
#include <zstd.h>

#define DESTINATION_BLOCK_SIZE (BGZF_MAX_BLOCK_SIZE+10000)

void TestZlib(const char* input_path, char* compressed_block, char* uncompressed_block)
{
	const char* compressed_path   = concat(input_path, ".zlib_compressed");
	const char* uncompressed_path = concat(input_path, ".zlib_uncompressed");

	// =====================
	// zlib compression pass
	// =====================

	printf("- starting zlib compression pass\n");

	FILE* in = fopen(input_path, "rb");
	FILE* out = fopen(compressed_path, "wb");

	if (!in) {
		printf("ERROR: Unable to open: %s\n", input_path);
		exit(1);
	}

	while (!feof(in))
	{
		int numBytesRead = (int)fread(uncompressed_block, 1, BGZF_MAX_BLOCK_SIZE, in);

		int comp_size = BGZF_MAX_BLOCK_SIZE;
		int ret = bgzf_compress(compressed_block, &comp_size, uncompressed_block, numBytesRead, 1);
		//printf("zlib compressed size: %d\n", comp_size);

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

	printf("- starting zlib decompression pass\n");

	in = fopen(compressed_path, "rb");
	out = fopen(uncompressed_path, "wb");

	while (!feof(in))
	{
		int comp_size;
		int numBytesRead = (int)fread((char*)&comp_size, 4, 1, in);
		if (numBytesRead == 0) break;

		numBytesRead = (int)fread(compressed_block, 1, comp_size, in);
		if (numBytesRead == 0) break;

		int uncompressed_size = bgzf_decompress(uncompressed_block, BGZF_MAX_BLOCK_SIZE, compressed_block, comp_size);
		fwrite(uncompressed_block, 1, uncompressed_size, out);
	}

	fclose(out);
	fclose(in);

	unlink(compressed_path);
	//unlink(uncompressed_path);
}

void TestQuickLZ(const char* input_path, char* compressed_block, char* uncompressed_block)
{
	const char* compressed_path   = concat(input_path, ".quicklz_compressed");
	const char* uncompressed_path = concat(input_path, ".quicklz_uncompressed");

	// ========================
	// quicklz compression pass
	// ========================

	printf("- starting quicklz compression pass\n");

	FILE* in = fopen(input_path, "rb");
	FILE* out = fopen(compressed_path, "wb");

	while (!feof(in))
	{
		int numBytesRead = (int)fread(uncompressed_block, 1, BGZF_MAX_BLOCK_SIZE, in);
		int comp_size = QuickLzCompress(uncompressed_block, numBytesRead, compressed_block, DESTINATION_BLOCK_SIZE);
		//printf("QuickLZ compressed size: %d\n", comp_size);

		fwrite((char*)&comp_size, 4, 1, out);
		fwrite(compressed_block, 1, comp_size, out);
	}

	fclose(out);
	fclose(in);

	// ==========================
	// quicklz decompression pass
	// ==========================

	printf("- starting quicklz decompression pass\n");

	in = fopen(compressed_path, "rb");
	out = fopen(uncompressed_path, "wb");

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

	unlink(compressed_path);
	//unlink(uncompressed_path);
}

void TestZstandard(const char* input_path, char* compressed_block, char* uncompressed_block)
{
	const char* compressed_path   = concat(input_path, ".zstandard_compressed");
	const char* uncompressed_path = concat(input_path, ".zstandard_uncompressed");

	// ==========================
	// Zstandard compression pass
	// ==========================

	printf("- starting Zstandard compression pass\n");

	FILE* in = fopen(input_path, "rb");
	FILE* out = fopen(compressed_path, "wb");

	while (!feof(in))
	{
		int numBytesRead = (int)fread(uncompressed_block, 1, BGZF_MAX_BLOCK_SIZE, in);

		//int compressed_size = (int)ZSTD_compressBound(numBytesRead);

		//if (compressed_size > DESTINATION_BLOCK_SIZE)
		//{
		//	printf("ERROR: The compressed data is potentially too large (%d) for the compressed block (%d).\n", compressed_size, DESTINATION_BLOCK_SIZE);
		//	exit(1);
		//}

		int comp_size = (int)ZSTD_compress(compressed_block, DESTINATION_BLOCK_SIZE, uncompressed_block, numBytesRead, 1);
		//printf("ZSTD compressed size: %d\n", comp_size);

		fwrite((char*)&comp_size, 4, 1, out);
		fwrite(compressed_block, 1, comp_size, out);
	}

	fclose(out);
	fclose(in);

	// ============================
	// Zstandard decompression pass
	// ============================

	printf("- starting Zstandard decompression pass\n");

	in = fopen(compressed_path, "rb");
	out = fopen(uncompressed_path, "wb");

	while (!feof(in))
	{
		int comp_size;
		int numBytesRead = (int)fread((char*)&comp_size, 4, 1, in);
		if (numBytesRead == 0) break;

		numBytesRead = (int)fread(compressed_block, 1, comp_size, in);
		if (numBytesRead == 0) break;

		int decompressed_size = (int)ZSTD_getDecompressedSize(compressed_block, comp_size);

		if (decompressed_size > BGZF_MAX_BLOCK_SIZE)
		{
			printf("ERROR: The uncompressed block is too small (%d) for the uncompressed data (%d).\n", BGZF_MAX_BLOCK_SIZE, decompressed_size);
			exit(1);
		}

		int uncompressed_size = (int)ZSTD_decompress(uncompressed_block, BGZF_MAX_BLOCK_SIZE, compressed_block, comp_size);
		fwrite(uncompressed_block, 1, uncompressed_size, out);
	}

	fclose(out);
	fclose(in);

	unlink(compressed_path);
	//unlink(uncompressed_path);
}

int main(int argc, char *argv[])
{
	if (argc != 2)
	{
		printf("USAGE: %s <input_path>\n", argv[0]);
		exit(1);
	}

	const char* input_path = argv[1];

	char* compressed_block   = malloc(DESTINATION_BLOCK_SIZE);
	char* uncompressed_block = malloc(BGZF_MAX_BLOCK_SIZE);

	TestZlib(input_path, compressed_block, uncompressed_block);
	TestQuickLZ(input_path, compressed_block, uncompressed_block);
	TestZstandard(input_path, compressed_block, uncompressed_block);

	free(compressed_block);
	free(uncompressed_block);

	return 0;
}
