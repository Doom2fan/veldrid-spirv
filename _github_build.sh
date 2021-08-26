#!/usr/bin/env bash

_PythonExePath=$(which python)
if [[ $_PythonExePath == "" ]]; then
    echo Build failed: could not locate python executable.
    exit 1
fi

bash ./ext/sync-shaderc.sh

mkdir build
cd build
cmake $SPIRVNATIVE_CMAKEOPTS -DPYTHON_EXECUTABLE=$_PythonExePath ../
make -j8 install