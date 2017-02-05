import sys
from os import listdir
import os

if (len(sys.argv) < 3):
	print "[Usage] python generateMiniSaFiles.py <Supplementary files Directory> <mini SA files directory>"
	
compressedSeq = sys.argv[1]
suppDirectory = sys.argv[2]
outDirectory = sys.argv[3]

fileList=["chr11_109286532_109286533",
"chr11_5226762_5226764",
"chr12_17600011_17600012",
"chr13_39724500_39724501",
"chr13_39724501_39724502",
"chr17_1280044_1280045",
"chr17_337122_337123",
"chr17_21416338_21416339",
"chr17_2363518_2363519",
"chr17_3221583_3221584",
"chr17_3712859_3712860",
"chr17_40701882_40701883",
"chr17_196316_196317",
"chr17_602009_602010",
"chr17_738094_738097",
"chr17_7024700_7024702",
"chr17_75516562_75516563",
"chr17_7529596_7529597",
"chr17_227472_227474",
"chr18_60817197_60817198",
"chr1_111241360_111241361",
"chr1_114713908_114713909",
"chr1_116066504_116066507",
"chr1_117623069_117623070",
"chr1_11794419_11794420",
"chr1_129010_129012",
"chr1_15274_15275",
"chr1_16759590_16759591",
"chr1_18849_18850",
"chr1_22099894_22099895",
"chr1_225404485_225404486",
"chr1_226071510_226071512",
"chr1_23796496_23796497",
"chr1_271618_271619",
"chr1_26282323_26282324",
"chr1_55052643_55052644",
"chr1_630211_630212",
"chr1_853876_853877",
"chr1_7905429_7905430",
"chr1_7961850_7961851",
"chr1_8061107_8061108",
"chr1_889689_889690",
"chr1_8323490_8323500",
"chr1_962774_962775",
"chr1_963222_963223",
"chr1_9245257_9245258",
"chr1_1048768_1048769",
"chr2_117807576_117807579",
"chr2_189769377_189769378",
"chr2_192322906_192322907",
"chr2_202201323_202201327",
"chr2_202201325_202201326",
"chr2_90301445_90301466",
"chr4_169966007_169966008",
"chr5_29786100_29786101",
"chr9_117713024_117713025",
"chrX_130220266_130220267",
"chrX_145823364_145823364",
"chrX_1500153_1500153",
"chrX_21487715_21487716",
"chrX_573733_573734",
"chrX_897237_897238"
]
for fileName in listdir(outDirectory):
	if ( fileName[-3:] != "nsa"):
		continue
	else :
		fileName = fileName[:-4]	# chopping off the ".nsa"
		
#for fileName in fileList:
	fields= fileName.split('_')
	
	if (len(fields) != 3):
		print "bad file name", fileName
		
	
	#print "generating Supplementary annotations for: ", fields
	refName = fields[0]
	start 	= fields[1]
	end 	= fields[2]
		
	command = "d:\\IlluminaAnnotationEngine\\Sandbox\\x64\\Release\\ExtractMiniSAdb.exe --in "+ suppDirectory +'\\'+refName+".nsa --begin "+start+" --end "+end+' --ref '+compressedSeq
	
	print command
	
	os.system(command)
	
	
