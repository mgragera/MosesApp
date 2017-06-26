#!/bin/bash

echo "Moses starts"

echo "Fichero1 ".$1
echo "Fichero2 ".$3 
echo  $2  $4

var1=$5

echo "UBICACION ".$var1

 ~/mosesdecoder/scripts/tokenizer/tokenizer.perl -l $2 \
    < $1    \
    > ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$2

~/mosesdecoder/scripts/tokenizer/tokenizer.perl -l $4 \
    < $3    \
    > ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$4

echo "Trucasing"

 ~/mosesdecoder/scripts/recaser/train-truecaser.perl \
     --model ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase-model.$2 --corpus     \
     ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$2
 ~/mosesdecoder/scripts/recaser/train-truecaser.perl \
     --model ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase-model.$4 --corpus     \
     ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$4

~/mosesdecoder/scripts/recaser/truecase.perl \
   --model ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase-model.$2         \
   < ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$2 \
   > ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase.$2
 ~/mosesdecoder/scripts/recaser/truecase.perl \
   --model ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase-model.$4         \
   < ~/VS/Moses/wwwroot/moses/corpus/$var1/tok.$4 \
   > ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase.$4

echo "Cleaning"

 ~/mosesdecoder/scripts/training/clean-corpus-n.perl \
    ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase $2 $4 \
    ~/VS/Moses/wwwroot/moses/corpus/$var1/$2-$4.clean 1 80


echo "TRAINING"

cd ~/VS/Moses/wwwroot/moses/lm/$var1
 ~/mosesdecoder/bin/lmplz -o 3 <~/VS/Moses/wwwroot/moses/corpus/$var1/truecase.$2 > arpa.$2

 ~/mosesdecoder/bin/build_binary \
   arpa.$2 \
   blm.$2

echo "Training the Translation System"

cd ~/VS/Moses/wwwroot/moses/working/$var1
 ~/mosesdecoder/scripts/training/train-model.perl -root-dir train \
 -corpus ~/VS/Moses/wwwroot/moses/corpus/$var1/$2-$4.clean                             \
 -f $2 -e $4 -alignment grow-diag-final-and -reordering msd-bidirectional-fe \
 -lm 0:3:$HOME/VS/Moses/wwwroot/moses/lm/$var1/blm.$2:8                          \
 -external-bin-dir ~/mosesdecoder/tools  > training.out

echo "TUNING"

 cd ~/VS/Moses/wwwroot/moses/corpus/$var1 
 ~/mosesdecoder/scripts/recaser/truecase.perl --model truecase.$4 \
   < ~/VS/Moses/wwwroot/moses/dev/newstest2011.tok.$4 > truecase2.$4
 ~/mosesdecoder/scripts/recaser/truecase.perl --model truecase.$2 \
   < ~/VS/Moses/wwwroot/moses/dev/newstest2011.tok.$2 > truecase2.$2


 cd ~/VS/Moses/wwwroot/moses/working/$var1
  ~/mosesdecoder/scripts/training/mert-moses.pl \
  ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase2.$2 ~/VS/Moses/wwwroot/moses/corpus/$var1/truecase2.$4 \
  ~/mosesdecoder/bin/moses ~/VS/Moses/wwwroot/moses/working/$var1/train/model/moses.ini --mertdir ~/mosesdecoder/bin/ --decoder-flags="-threads 4"
  
#cd ~/working/$var1  
#~/mosesdecoder/scripts/training/mert-moses.pl \
#~/corpus/$var1/truecase.$2 ~/corpus/$var1/truecase.$4\
#~/mosesdecoder/bin/moses ~/working/$var1/train/model/moses.ini --working-dir ~/working/$var1 --rootdir ~/mosesdecoder/scripts/

echo "Binarising model"
 cd ~/VS/Moses/wwwroot/moses/working/$var1/
 mkdir binarised-model

~/mosesdecoder/bin/processPhraseTableMin -in ~/VS/Moses/wwwroot/moses/working/$var1/train/model/phrase-table.gz -nscores 4 -out binarised-model/phrase-table

~/mosesdecoder/bin/processLexicalTableMin -in ~/VS/Moses/wwwroot/moses/working/$var1/train/model/reordering-table.wbe-msd-bidirectional-fe.gz -out binarised-model/reordering-table

cp ~/VS/Moses/wwwroot/moses/working/$var1/mert-work/moses.ini binarised-model
cd binarised-model
sed -i 's/PhraseDictionaryMemory/PhraseDictionaryCompact/g' moses.ini
sed -i "s,/home/mgragera/VS/Moses/wwwroot/moses/working/${var1}/train/model/phrase-table.gz,/home/mgragera/VS/Moses/wwwroot/moses/working/$var1/binarised-model/phrase-table.minphr,g" moses.ini
sed -i "s,/home/mgragera/VS/Moses/wwwroot/moses/working/${var1}/train/model/reordering-table.wbe-msd-bidirectional-fe.gz,/home/mgragera/VS/Moses/wwwroot/moses/working/$var1/binarised-model/reordering-table,g" moses.ini



