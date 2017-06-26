#!/bin/bash


echo "${@:2}" | ~/mosesdecoder/bin/moses -f ~/working/$1/mert-work/moses.ini
