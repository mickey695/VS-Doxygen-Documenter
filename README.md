# Doxygen Documenter #
Last updated: 28/07/2018

This is a VS extension that allows creating Doxygen templates automatically above a function definition

## Features still needing implementation ##
* Use editors' line seperator instead of LF
* Use correct indentation for the documentation(+try to guess the correct tabbing style for indentation of the documentation)
* Think of a way of implementing an option for generating `@throws` blocks for languages with exceptions (Will probably need to do language analysis because VS SDK doesn't export it by itself)
* Make documentation style configurable
* Just make everything configurable
    
## Notes ##
The extension was only tested with VS 2017 Community
I have no clue if it will work on any other edition, although there is no reason it won't

## Contribution ##
You would like to implement one of the above mentiond things or test them ?
Submit a PR and I will look into it when I have time