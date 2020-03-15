Hi there! 

Thanks for downloading this code and opening up this file. The goal of this 
project is to provide an annotated reference implementation of Microsoft's 
TrueSkill algorithm. 

I describe the philosophy and the buildup of the math involved in my blog post
"Computing Your Skill" available at moserware.com.	   

In addition, there is a math paper that goes along with the blog post that explains 
most of the more technical concepts. 

This project isn't intended to win performance tests, it's meant to be read 
and understood. If you see ways to improve its clarity, please submit a patch.

If you just want to use the TrueSkill algorithm, simply use the TrueSkillCalculator
class and enjoy. If you need examples, please look in the UnitTests\TrueSkill folder.

If you want to understand the inner workings of the algorithm and implement it 
yourself, look in the Skills\TrueSkill folder. There are three separate 
implementations of the algorithm in increasing levels of difficulty:

	1. TwoPlayerTrueSkillCalculator.cs is the easiest to follow and implement. It uses
	   the simple equations directly from the TrueSkill website.
	2. TwoTeamTrueSkillCalculator.cs is slightly more complicated than the two player 
	   version and supports two teams that have at least one player each. It extends 
	   the equations on the website and incorporates some things implied in the paper.
	3. FactorGraphTrueSkillCalculator.cs is a wholly different animal than the first two
	   and it is at least an order of magnitude more complex. It implements the complete 
	   TrueSkill algorithm and builds up a "factor graph" composed of several layers. 
	   Each layer is composed of "factors", "variables", and "messages" between the two. 
	   
	   Work happens on the factor graph according to a "schedule" which can either be
	   a single step (e.g. sending a message from a factor to a variable) or a sequence of
	   steps (e.g. everything that happens in a "layer") or a loop where the schedule runs
	   until values start to stabilize (e.g. the bottom layer is approximated and runs until
	   it converges)
	   
TrueSkill is more general than the popular Elo algorithm. As a comparison, I implemented
the Elo algorithm using the both the bell curve (Gaussian) and curve that the FIDE chess
league uses (logistic curve). I specifically implemented them in a way to show how the
only difference among these Elo implementations is the curve. I also implemented the 
"duelling" Elo calculator as implied in the paper.

Everything else was implemented to support these classes. Note that a "player" can be an
arbitrary class. However, if that player class supports the "ISupportPartialPlay" or
"ISupportPartialUpdate" interfaces, you can add these extra parameters. The only calculator
that uses this info is the factor graph implementation. See those files for more details.

I use this code personally to rank around 45 people, so it's important that it's accurate.
Please let me know if you find errors. Bug fix patches are strongly encouraged! Also, feel
free to fork the project for different language implementations.

I'd love to hear from you via comments on the "Computing Your Skill" blog post.

Have fun and enjoy!

Jeff Moser <jeff@moserware.com>