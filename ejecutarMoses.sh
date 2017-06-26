#!/bin/bash


echo "Moses starts"

echo "Fichero1 ".$1
echo "Fichero2 ".$3 
echo  $2  $4


./moses.sh $1 $2 $3 $4 $5 &> moses.out &
