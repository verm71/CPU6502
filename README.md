# CPU6502

## Goals

I always think emulating the I/O is the difficult part of writing an emulator. If I wanted to make a full-featured C64 emulator, I'd still personally stick to using VICE, personally. So doing a C64 emulator would be pretty useless in this day and age.

The goal of this project was to be able to write a 6502/6510 CPU  good enough to execute code and see its effect on memory, possibly also talking to I/O devices. Emulating the VIC-II chip and CIAs (which in the end may not be necessary) is quite a challenge. I may cut some corners just to be able to see the execution of the kernal and display a BASIC READY prompt. I don't know how far I want to take it.  The order in which I'm implementing the opcodes is in the order they are used during the execution of the kernal at its initialization vector stored at $FFEC.

It's fun to watch and step through, but once you've programmed a couple dozen opcodes, you get the jist of things. The magic of the novelty is gone and you reach a fork in the road where you either continue for the sake of becoming a more full-featured emulator, or you stop. Programming more opcodes for the sake of programming opcodes wears off. There's only so much LDA, STA and BNE you need to step through to be satisfied that what you've done was a fun exercise. At that point, you take a step back and decide, do I implement the remaining 40+ opcodes and all their addressing modes, and if so, why? So that the entire kernal and BASIC ROM can boot up? Then what? There's a make or break point where the excitement of the novelty wears off and proceeding forward means a commitment to also get into I/O, and other details like cycles, timers, screen output, keyboad input...  You get it. 
