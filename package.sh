#!/bin/bash
# Copyright © 2013-2014, Elián Hanisch
#
# This script is for packaging everything into a zip file.

NAME="TSUtils"
DIR="Package/$NAME"

# Get plugin version
VERSION="$(grep AssemblyVersion Properties/AssemblyInfo.cs)"
VERSION=${VERSION/*AssemblyVersion *(\"/}
VERSION=${VERSION/.\*\")*/}

rm -rf "$DIR"
mkdir -vp "$DIR"

# copy plugins
mkdir -vp "$DIR/Plugins"
cp -v bin/Release/*.dll "$DIR/Plugins"

# copy sources
mkdir -vp "$DIR/Sources/"
cp -v *.cs "$DIR/Sources"
cp -vr Properties "$DIR/Sources"

# copy documentation
cp -v *.asciidoc "$DIR"

# copy other files
mkdir -vp "$DIR/Textures"
cp -v Textures/tsutils_24.png "$DIR/Textures"
cp -v *.version "$DIR"

# make package
cd Package
ZIPNAME="${NAME}_v${VERSION}.zip"
rm -f "$ZIPNAME"
zip -r "$ZIPNAME" "$NAME"

echo "Package ${ZIPNAME} built."

