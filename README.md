# ECS-IAUS-sytstem
Infinite Axis Utility System for Unity 
The utility system works by identifying options available to the AI and selecting the best option by scoring each option based on the circumstances. This has proven a remarkable well-working method for several reasons.

Simple to Design - The Utility AI can often be designed in natural language, which makes it easy for the AI programmer to speak with game designers. There is no need to talk about arcane concepts such as conditions, states, sequences and decorators. Instead you can explain the intended AI behavior in terms such as “If the AI is under fire, prioritize finding cover”. Note how fuzzy terms - such as “prioritize” - can be used, which comes natural to human conversation.

Easily Extendable - The rules - often referred to as scorers - can easily be added on top of the existing AI. Contrary to e.g. finite state machines, there are no important relationships to break. Instead scorers are simply added on top of existing scorers making it easy to extend AI functionality and fidelity.

Better Quality - The simplicity of design and the ease by which the AI can be extended vastly reduces bugs and dramatically improves productivity. This in turn leaves more room for developing more complex and well-behaving AI within the given budget and timeframe, which overall improves the quality of the AI.

This is my attempt at creating Dave Mark's Infinite Axis Utility System in Unity3d using DOTS.

What is an Infinite Axis Utility System?

In a nutshell it is a system that returns an Action to perform with the highest value. It’s bascially a list of Actions and each Action has a list of “Axis”. Dave Mark did a talk on it at GDC a couple years ago and that’s where I heard about it. I think he also mentioned a Cat and Laser pointer game in the same talk actually which was the initial seed idea of Lol Cats I Can Has Lazer, though our game went it’s own design direction.

Here is a link to the talks:

http://intrinsicalgorithm.com/IAonAI/2013/02/both-my-gdc-lectures-on-utility-theory-free-on-gdc-vault/

https://www.gdcvault.com/play/1018040/Architecture-Tricks-Managing-Behaviors-in starts at 33 min mark

In an effort to get an Project Rebirth to a Release Candidate, this project will remain  on Entities 0.51 for the time being. Once a stable build of Project Rebirth is a achieved and Entities 1.0 is stable with full feature release, refactoring update will began. 

This project depend the following Paid Assets. If you know of good full feature open source replacements, please log an issue with a link to the repo and they will be considered for future updates. 
Love/Hate by Pixel Crushers https://assetstore.unity.com/packages/tools/ai/love-hate-33063
Love/Hate is a relationship and personality simulator for Unity. It models characters’ feelings about each other using emotional states and value-based judgment of deeds, including gameplay actions done by the player. This system is tied into the influence grid. This will allow NPC to determine if the are in a hostile or friend area and evaluate location for cover, escape, and/or support.

New blog site in development ghost.dreamersinc.studio

Copyright 2019,2020,2021,2022 Dreamers Inc Studios Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
