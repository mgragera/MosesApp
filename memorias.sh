#!/bin/bash

cd $1

cat *.$2 > merged-file.$2
cat *.$3 > merged-file.$3
