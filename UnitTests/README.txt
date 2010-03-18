These tests were written using NUnit 2.5.2 that is available for download
at:

http://sourceforge.net/projects/nunit/files/NUnit%20Version%202/NUnit-2.5.2.9222.msi/download

If you have a different version or setup, you'll need to update the path under 
the UnitTests project properties by right clicking on UnitTests and then
click "properties" and then click the "debug" tab. The "start external program"
points to the NUnit test runner.

I did it this way so you didn't need more than the express version of 
Visual Studio to run. If you have a fancy test runner already, feel
free to use that. 

Additionally, it should be easy to update the tests to your tool
of choice.

Finally, realize that these tests test *all* of the calculators 
implementations. For that reason, they create a new instance of
a particular calculator. If you're using this code in your application,
you can just use the convenience helper class of "TrueSkillCalculator"
that has static methods. If you do that, you won't have to worry
about creating your own instances.