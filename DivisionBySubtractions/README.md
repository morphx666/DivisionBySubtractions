# DivisionBySubtractions

Simple implementation of a function to divide very large numbers with an arbitrary precision using successive subtractions.
For example, 128/8 is the same as subtracting 8 from 128, 16 times.

Here's the result of dividing 915.27 by 37768.2313358 using 32 digits of precision:
![DBS](https://xfx.net/stackoverflow/dbs/dbs01.png)

### Usage and command line arguments
**dbs** dividend divisor [precision] [r]
* dividend: Any number from Double.Min to Double.Max
* divisor: Any number from Double.Min to Double.Max
* precision: A positive integer representing the number of decimal places
* r: If included, the result will be rounded
