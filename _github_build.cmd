call ext/sync-shaderc.cmd

mkdir build
cd build
cmake %SPIRVNATIVE_CMAKEOPTS% ../
msbuild /maxCpuCount /v:m /p:Configuration=Release INSTALL.vcxproj