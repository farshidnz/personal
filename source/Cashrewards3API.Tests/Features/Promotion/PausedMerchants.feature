Feature: Paused merchants for web campaigns

https://shopgoau.atlassian.net/browse/CPS-1482

@tag1
Scenario Outline: Paused Merchants in Merchant campaign page
	Given the feature flag for Merchant Pause is '<flag status>'
	And paused status for merchant <merchant id> is '<pause status>'
	And paused status for offer id <offer id> with merchant id <merchant id> is '<pause status>'
	When promotions are requested
	Then merchant id <merchant id> and offer id <offer id> should '<in result>' be in the result
		
Examples:
	| flag status | pause status | merchant id | offer id | in result |
	| off         | false        | 1004728     | 477100   | yes       |
	| off         | true         | 1004728     | 477100   | yes       |
	| on          | false        | 1004728     | 477100   | yes       |
	| on          | true         | 1004728     | 477100   | not       |

