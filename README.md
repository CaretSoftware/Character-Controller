# Character Controller 

Character Conteroller using a Scriptable Object State Machine.

## Table of Contents

- [Character Controller](#character-controller)
  - [Table of Contents](#table-of-contents)
  - [About the Project](#about-the-project)
  - [Prerequisites](#prerequisites)
  - [Usage](#usage)
  - [License](#license)
  - [Contact](#contact)

## About the Project

Unitys Character Controller allows for rudimentary physics interaction and movement.
The controller is powered by a state machine where the designer can drop in differently designed movement behaviours (jump, jump-to-ledge, walk, run, climb, idle, click-to-move, combat, etc.) and set variables (i.e. jump height) directly in the inspector.
Some amount of editor scripting was done to allow for to convenience of seting the variables directly int the inspector of the character.

The intention with this project was to have a system of designing behaviours that could be interchangeable and to quickly assemble characters (both player and enemy characters) with different movement abilities.

This repository is meant to serve as inspiration for others (and to learn from my mistakes).  
Scriptable Objects was a neat idea for such a concept that could be helpful for desingers but they are not meant to be used this way which caused some interresting problems to overcome but ultimately leads to some friction in development for programmers.

![image](https://github.com/CaretSoftware/Character-Controller/assets/69549081/a4870080-7970-4cf0-af38-0058a4951f60)

### Prerequisites

Unity 2022.3.12f1

## Usage

WASD to move, Space to jump, Q & E to swap characters.  
Mouse to look around.  
One of the characters are click to move.  
(Gamepad controls shown on UI)  

## License

MIT No Attribution

Copyright 2024 Patrik Bergsten

Permission is hereby granted, free of charge, to any person obtaining a copy of 
this software and associated documentation files (the "Software"), to deal in 
the Software without restriction, including without limitation the rights to use, 
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
the Software, and to permit persons to whom the Software is furnished to do so.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS 
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES 
OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Contact

- Email: pbergsten@me.com
- Website: [patrikbergsten.com](https://www.patrikbergsten.com)
- Itch.io: [bergsten.itch.io](https://bergsten.itch.io)
- Twitter: [@patrik_bergsten](https://twitter.com/patrik_bergsten)
- GitHub: [CaretSoftware](https://github.com/CaretSoftware)
