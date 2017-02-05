#pragma once

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <zlib.h>

#define BGZF_MAX_BLOCK_SIZE 0x10000

// grab the zlib version
char* GetVersion();
// inflate the block in source array into the destination array
int bgzf_decompress(char* destination, int destinationLen, char* source, int sourceLen);