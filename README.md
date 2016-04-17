# Transrender

Renders sprites for Transport Tycoon from MagicaVoxel files. (May work for other transport and even non-transport games too).

The core of this is based on an extremely old voxel renderer I cobbled together out of some GPLed Javascript code and a few
hastily spiked ideas - code quality is Not Good and it's in desperate need of some love and a refactor but it does what it
does and mostly works.

The renderer is horrendously slow for what it does, but thanks to the wonders of the .net framework it is able to take advantage
of multi-core CPUs to make rendering a large number of sprites slightly less slow.

## Running

Dump all of the MagicaVoxel .vox files you want rendered in the same folder as the .exe file. They will need to have the Transport
Tycoon Deluxe palette otherwise bad and odd things will happen to their colours.

Files will be rendered to PNG in the same folder.

## Licence

Almost certainly based on GPL v2 code at some point, so therefore transitively GPL v2. See license.txt for details.
