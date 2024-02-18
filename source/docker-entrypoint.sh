#!/bin/bash

echo "Restarting nginx"
service nginx start
cd /app
./Cashrewards3API



