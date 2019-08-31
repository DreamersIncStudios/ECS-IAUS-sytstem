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