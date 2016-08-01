#pragma once

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <zlib.h>

#define BGZF_MAX_BLOCK_SIZE 0x10000

// grab the zlib version
char* GetVersion();
// inflate the block in compressed_block into uncompressed_block
int uncompress_block(char* uncompressed_block, char* compressed_block, int block_length);
