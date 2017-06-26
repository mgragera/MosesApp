#!/bin/bash


echo "${@:2}" | ~/mosesdecoder/bin/moses -f ~/VS/Moses/wwwroot/moses/working/$1/binarised-model/moses.ini
