<img align="left" src="https://github.com/Illumina/Nirvana/wiki/images/NirvanaLogo.png" width="300" />
<br clear=left>

Nirvana provides **clinical-grade annotation of genomic variants** (SNVs, MNVs, insertions, deletions, indels, and SVs (including CNVs). It can be run as a stand-alone package or integrated into larger software tools that require variant annotation.

The input to Nirvana are VCFs and the output is a structured JSON representation of all annotation and sample information (as extracted from the VCF). Optionally, a subset of the annotated data is available in VCF and/or gVCF files. Nirvana handles multiple alternate alleles and multiple samples with ease.

The software is being developed under a rigorous SDLC and testing process to ensure accuracy of the results and enable embedding in other software with regulatory needs. Nirvana uses a continuous integration pipeline where millions of variant annotations are monitored against baseline values on a daily basis.

Backronym: **NI**mble and **R**obust **VA**riant a**N**not**A**tor
<br clear=left>

## Resources

* [Getting Started](https://github.com/Illumina/Nirvana/wiki/Getting-Started)
* [Wiki](https://github.com/Illumina/Nirvana/wiki)
* [Release Notes](https://github.com/Illumina/Nirvana/releases)
