# Nirvana Variant Annotator

<img align="right" src="https://github.com/Illumina/Nirvana/wiki/images/NirvanaCartoon.png" width="256" />

Nirvana provides **clinical-grade annotation of genomic variants** (SNVs, MNVs, insertions, deletions, indels, and SVs (including CNVs). It can be run as a stand-alone package or integrated into larger software tools that require variant annotation.

The input to Nirvana are VCFs and the output is a structured JSON representation of all annotation and sample information (as extracted from the VCF). Optionally, a subset of the annotated data is available in VCF and/or gVCF files. Nirvana handles multiple alternate alleles and multiple samples with ease.

The software is being developed under a rigorous SDLC and testing process to ensure accuracy of the results and enable embedding in other software with regulatory needs. Nirvana uses a continuous integration pipeline where millions of variant annotations are monitored against baseline values on a daily basis.

Backronym: **NI**mble and **R**obust **VA**riant a**N**not**A**tor

<br clear=left>

## Resources

* [Release Notes](https://github.com/Illumina/Nirvana/releases)
* [Wiki](https://github.com/Illumina/Nirvana/wiki)

## Installing

Nirvana is written in C# and targeted for 64-bit operating systems.

### Linux

Many Linux distributions already have mono installed. If you need to install it, however, please refer to the [Install Mono on Linux](http://www.mono-project.com/docs/getting-started/install/linux/) page.

For example, the following should suffice on Ubuntu:

```Bash
sudo apt-get install mono-complete
```

### Windows

No additional dependencies are required. Nirvana can be executed at the command-line.