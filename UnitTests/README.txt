Realize that these tests test *all* of the calculators 
implementations. For that reason, they create a new instance of
a particular calculator. If you're using this code in your application,
you can just use the convenience helper class of "TrueSkillCalculator"
that has static methods. If you do that, you won't have to worry
about creating your own instances.