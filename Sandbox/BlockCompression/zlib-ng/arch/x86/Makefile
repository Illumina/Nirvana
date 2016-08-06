# Makefile for zlib
# Copyright (C) 1995-2013 Jean-loup Gailly, Mark Adler
# For conditions of distribution and use, see copyright notice in zlib.h

CC=gcc
CFLAGS=-O3  -Wall -D_LARGEFILE64_SOURCE=1 -DHAVE_HIDDEN -DHAVE_BUILTIN_CTZL -DMEDIUM_STRATEGY -DX86_64 -DX86_NOCHECK_SSE2 -DUNALIGNED_OK -DUNROLL_LESS -DX86_CPUID -DX86_SSE2_FILL_WINDOW -DX86_SSE4_2_CRC_HASH -DX86_PCLMULQDQ_CRC -DX86_QUICK_STRATEGY
SFLAGS=-O3  -Wall -fPIC -D_LARGEFILE64_SOURCE=1 -DHAVE_HIDDEN -DHAVE_BUILTIN_CTZL -DMEDIUM_STRATEGY -DX86_64 -DX86_NOCHECK_SSE2 -DUNALIGNED_OK -DUNROLL_LESS -DX86_CPUID -DX86_SSE2_FILL_WINDOW -DX86_SSE4_2_CRC_HASH -DX86_PCLMULQDQ_CRC -DX86_QUICK_STRATEGY
INCLUDES=-I/home/mstromberg/source/BlockCompression.old2/zlib-ng/arch/x86 -I/home/mstromberg/source/BlockCompression.old2/zlib-ng

SRCDIR=/home/mstromberg/source/BlockCompression.old2/zlib-ng/arch/x86
SRCTOP=/home/mstromberg/source/BlockCompression.old2/zlib-ng

all: x86.o x86.lo fill_window_sse.o fill_window_sse.lo deflate_quick.o deflate_quick.lo insert_string_sse.o insert_string_sse.lo crc_folding.o crc_folding.lo

x86.o:
	$(CC) $(CFLAGS) $(INCLUDES) -c -o $@ $(SRCDIR)/x86.c

x86.lo:
	$(CC) $(SFLAGS) $(INCLUDES) -c -o $@ $(SRCDIR)/x86.c

fill_window_sse.o:
	$(CC) $(CFLAGS) -msse2 $(INCLUDES) -c -o $@ $(SRCDIR)/fill_window_sse.c

fill_window_sse.lo:
	$(CC) $(SFLAGS) -msse2 -DPIC $(INCLUDES) -c -o $@ $(SRCDIR)/fill_window_sse.c

deflate_quick.o:
	$(CC) $(CFLAGS) -msse4 $(INCLUDES) -c -o $@ $(SRCDIR)/deflate_quick.c

deflate_quick.lo:
	$(CC) $(SFLAGS) -msse4 -DPIC $(INCLUDES) -c -o $@ $(SRCDIR)/deflate_quick.c

insert_string_sse.o:
	$(CC) $(CFLAGS) -msse4 $(INCLUDES) -c -o $@ $(SRCDIR)/insert_string_sse.c

insert_string_sse.lo:
	$(CC) $(SFLAGS) -msse4 -DPIC $(INCLUDES) -c -o $@ $(SRCDIR)/insert_string_sse.c

crc_folding.o:
	$(CC) $(CFLAGS) -mpclmul -msse4 $(INCLUDES) -c -o $@ $(SRCDIR)/crc_folding.c

crc_folding.lo:
	$(CC) $(SFLAGS) -mpclmul -msse4 -DPIC $(INCLUDES) -c -o $@ $(SRCDIR)/crc_folding.c


mostlyclean: clean
clean:
	rm -f *.o *.lo *~ \
	rm -rf objs
	rm -f *.gcda *.gcno *.gcov

distclean:
	rm -f Makefile
