#!/usr/bin/perl

use File::Find;
use Data::Dumper;
use Storable qw(fd_retrieve dclone);
use Compress::Zlib;
use MIME::Base64;

use strict;

$Data::Dumper::Sortkeys = 1; # Sort the keys in the output
$Data::Dumper::Deepcopy = 1; # Enable deep copies of structures

my @transcriptFiles = ();
my @regulatoryFiles = ();

my $numArgs = @ARGV;

if($numArgs != 1) {
	print "USAGE: ConvertCacheMatrix.pl <input VEP directory>\n";
	exit 1;
}

my ($srcDir) = @ARGV;

if(! -d $srcDir) {
	print "ERROR: The directory ($srcDir) does not exist.\n";
	exit 1;
}

find(\&wanted, $srcDir);

foreach my $transcriptPath (@transcriptFiles) {

	print "- Dumping $transcriptPath.\n";
	
    open my $fh, "zcat ".$transcriptPath." |";
    my $cache;
    $cache = fd_retrieve($fh);
    close $fh;
	
	my $outputCache = dclone($cache);
	
	my $newPath = $transcriptPath;
	$newPath =~ s/\.gz$/_transcripts_data_dumper.txt.gz/g;

	# loop through each reference sequence
	foreach my $refSeq (keys %{$cache}) {

		print "refSeq: $refSeq\n";

		# loop through each transcript
		my $numTranscripts = scalar @{$cache->{$refSeq}};
		print "# transcripts: $numTranscripts\n";
		
		for(my $transcriptIndex = 0; $transcriptIndex < $numTranscripts; $transcriptIndex++) {
		
			print "- evaluating transcript ".($transcriptIndex + 1)."... ";
		
			# evaluate the SIFT entry
			my $sift = $cache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'sift'}->{'matrix'};
			
			if(defined($sift)) {
				my $dest = Compress::Zlib::memGunzip($sift) 
					or die "Cannot uncompress SIFT matrix: $gzerrno";
				
				$outputCache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'sift'}->{'matrix'} = encode_base64($dest, "");
			}
			
			# evaluate the PolyPhen humvar entry
			my $polyphen = $cache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'polyphen_humvar'}->{'matrix'};
			
			if(defined($polyphen)) {
				my $dest = Compress::Zlib::memGunzip($polyphen) 
					or die "Cannot uncompress PolyPhen matrix: $gzerrno";
				
				$outputCache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'polyphen_humvar'}->{'matrix'} = encode_base64($dest, "");
			}
			
			# evaluate the PolyPhen humdiv entry
			my $polyphenDiv = $cache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'polyphen_humdiv'}->{'matrix'};
			
			if(defined($polyphenDiv)) {
				my $dest = Compress::Zlib::memGunzip($polyphenDiv) 
					or die "Cannot uncompress PolyPhen humdiv matrix: $gzerrno";
				
				$outputCache->{$refSeq}[$transcriptIndex]->{'_variation_effect_feature_cache'}->{'protein_function_predictions'}->{'polyphen_humdiv'}->{'matrix'} = encode_base64($dest, "");
			}
			
			print "finished.\n";
		}
	}
	
	open (my $MPS, "| /bin/gzip -9 -c > $newPath") or die "error starting gzip $!";
	print $MPS Dumper($outputCache);
	close $MPS;
}

foreach my $regulatoryPath (@regulatoryFiles) {

	print "- Dumping $regulatoryPath.\n";
	
    open my $fh, "zcat ".$regulatoryPath." |";
    my $cache;
    $cache = fd_retrieve($fh);
   close $fh;
 	
	my $newPath = $regulatoryPath;
	$newPath =~ s/\.gz$/_regulatory_regions_data_dumper.txt.gz/g;
	
	open (my $MPS, "| /bin/gzip -9 -c > $newPath") or die "error starting gzip $!";
	print $MPS Dumper($cache);
	close $MPS;
}

# ========================================

sub wanted {

	my $filePath = $File::Find::name;
	
	if($filePath =~ /data_dumper/) { return; }
	
	if($filePath =~ /_reg.gz$/) { 
		push(@regulatoryFiles, $filePath) if -f $filePath;
		return;
	}
	
	if($filePath =~ /_var.gz$/) { return; }

	if($filePath =~ /.gz$/) {
		push(@transcriptFiles, $filePath) if -f $filePath;
		return; 
	}
}
