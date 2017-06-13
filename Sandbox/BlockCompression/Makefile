# define our object and binary directories
TOP_DIR=$(PWD)
OBJ_DIR=obj
BIN_DIR=bin

# define our directories
HTSLIB_DIR=htslib
ILLUMINA_DIR=illumina
QUICKLZ_DIR=quicklz-1.5.0
ZLIB_DIR=zlib-ng
TBC_DIR=TestBlockCompression
ZSTD_DIR=zstd

# define our include directories
HTSLIB_INCLUDE=-I $(TOP_DIR)/$(HTSLIB_DIR) -I $(TOP_DIR)/$(HTSLIB_DIR)/htslib
ILLUMINA_INCLUDE=-I $(TOP_DIR)/$(ILLUMINA_DIR)
QUICKLZ_INCLUDE=-I $(TOP_DIR)/$(QUICKLZ_DIR)
ZLIB_INCLUDE=-I $(TOP_DIR)/$(ZLIB_DIR)
ZSTD_INCLUDE=-I $(TOP_DIR)/$(ZSTD_DIR)/lib -I $(TOP_DIR)/$(ZSTD_DIR)/lib/common

# define our output library
SHAREDLIB=$(BIN_DIR)/libBlockCompression.so
SHAREDLIBM=$(SHAREDLIB).1

# define our output program
TESTBLOCKCOMPRESSION=$(BIN_DIR)/TestBlockCompression
LIBS=-L$(TOP_DIR)/$(BIN_DIR) -Wl,-rpath=$(TOP_DIR)/$(BIN_DIR) -lBlockCompression -lpthread

# define some default flags
CC=gcc
CFLAGS=-O3 -fPIC -fprofile-use
MARCH=-mavx
LDFLAGS=-Wl,-Bstatic -static-libgcc -Wl,-Bdynamic
LDSHARED=gcc -shared -Wl,-soname,$(SHAREDLIBM),--version-script,BlockCompression.map
DEFINES=-D_LARGEFILE64_SOURCE=1 -DHAVE_HIDDEN -DHAVE_BUILTIN_CTZL -DMEDIUM_STRATEGY -DX86_64 -DX86_NOCHECK_SSE2 -DUNALIGNED_OK -DUNROLL_LESS -DX86_CPUID -DX86_SSE2_FILL_WINDOW -DX86_SSE4_2_CRC_HASH -DX86_PCLMULQDQ_CRC -DX86_QUICK_STRATEGY

# define our source files
SOURCES=$(ILLUMINA_DIR)/BlockGZipUtils.c \
	$(HTSLIB_DIR)/bgzf.c \
	$(HTSLIB_DIR)/hfile.c \
	$(QUICKLZ_DIR)/quicklz.c \
	$(ZLIB_DIR)/adler32.c \
	$(ZLIB_DIR)/crc32.c \
	$(ZLIB_DIR)/deflate.c \
	$(ZLIB_DIR)/deflate_fast.c \
	$(ZLIB_DIR)/deflate_medium.c \
	$(ZLIB_DIR)/deflate_slow.c \
	$(ZLIB_DIR)/match.c \
	$(ZLIB_DIR)/inffast.c \
	$(ZLIB_DIR)/inflate.c \
	$(ZLIB_DIR)/inftrees.c \
	$(ZLIB_DIR)/trees.c \
	$(ZLIB_DIR)/zutil.c \
	$(ZLIB_DIR)/arch/x86/x86.c \
	$(ZLIB_DIR)/arch/x86/fill_window_sse.c \
	$(ZLIB_DIR)/arch/x86/insert_string_sse.c \
	$(ZLIB_DIR)/arch/x86/crc_folding.c \
	$(ZLIB_DIR)/arch/x86/deflate_quick.c \
	$(ZSTD_DIR)/lib/common/entropy_common.c \
	$(ZSTD_DIR)/lib/common/fse_decompress.c \
	$(ZSTD_DIR)/lib/common/xxhash.c \
	$(ZSTD_DIR)/lib/common/zstd_common.c \
	$(ZSTD_DIR)/lib/compress/fse_compress.c \
	$(ZSTD_DIR)/lib/compress/huf_compress.c \
	$(ZSTD_DIR)/lib/compress/zstd_compress.c \
	$(ZSTD_DIR)/lib/decompress/huf_decompress.c \
	$(ZSTD_DIR)/lib/decompress/zbuff_decompress.c \
	$(ZSTD_DIR)/lib/decompress/zstd_decompress.c

OBJS=$(addprefix obj/,$(notdir $(SOURCES:.c=.o))) 

all: $(SHAREDLIB) $(TESTBLOCKCOMPRESSION)
	@true

clean:
	@-rm -f $(SHAREDLIB) $(OBJS)

$(SHAREDLIB): $(OBJS)
	@test -d $(OBJ_DIR) || mkdir $(OBJ_DIR)
	@test -d $(BIN_DIR) || mkdir $(BIN_DIR)
	@echo "- creating the shared library"
	@$(LDSHARED) $(CFLAGS) $(DEFINES) -o $@ $(OBJS) -lc $(LDFLAGS)

$(TESTBLOCKCOMPRESSION): $(SHAREDLIB)
	@echo "- creating TestBlockCompression"
	$(CC) $(CFLAGS) $(MARCH) $(ZLIB_INCLUDE) $(HTSLIB_INCLUDE) $(ILLUMINA_INCLUDE) $(QUICKLZ_INCLUDE) $(ZSTD_INCLUDE) -o $(TESTBLOCKCOMPRESSION) $(TBC_DIR)/Test.c $(LIBS)

obj/%.o : $(ILLUMINA_DIR)/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZLIB_INCLUDE) -c $< -o $@

obj/%.o : $(HTSLIB_DIR)/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZLIB_INCLUDE) $(ILLUMINA_INCLUDE) $(HTSLIB_INCLUDE) -c $< -o $@

obj/%.o : $(QUICKLZ_DIR)/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) -c $< -o $@

obj/%.o : $(ZLIB_DIR)/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZLIB_INCLUDE) -c $< -o $@

obj/%.o : $(ZLIB_DIR)/arch/x86/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZLIB_INCLUDE) -c $< -o $@

obj/crc_folding.o : $(ZLIB_DIR)/arch/x86/crc_folding.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) -mpclmul -msse4 $(DEFINES) $(ZLIB_INCLUDE) -c $< -o $@

obj/%.o : $(ZSTD_DIR)/lib/common/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZSTD_INCLUDE) -c $< -o $@

obj/%.o : $(ZSTD_DIR)/lib/compress/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZSTD_INCLUDE) -c $< -o $@

obj/%.o : $(ZSTD_DIR)/lib/decompress/%.c
	@mkdir -p $(dir $@)
	@echo "- compiling" $(*F).c
	@$(CC) $(CFLAGS) $(MARCH) $(DEFINES) $(ZSTD_INCLUDE) -c $< -o $@
