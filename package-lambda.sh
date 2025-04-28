#!/bin/bash
# 打包 Lambda 函数
dotnet lambda package -c Release -f net9.0 -o deployment-package.zip