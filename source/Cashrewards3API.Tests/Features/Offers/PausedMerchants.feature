Feature: Paused merchants

https://shopgoau.atlassian.net/browse/CPS-1581

@tag1
Scenario Outline: Paused merchants for web and mob
	Given Feature Flag for Merchant Pause Is '<flagstatus>'
	And one or more '<offertype>' offers are listed with Merchant Paused status as '<pausestatus>'
	When all Offers for '<platform>' of '<offertype>' are requested
	Then the result list does '<containassertion>' contain listed offers

Examples:
	| platform | offertype | flagstatus | pausestatus | containassertion |
	| web      | Special   | off        | false       |                  |
	| web      | Special   | off        | true        |                  |
	| web      | Special   | on         | false       |                  |
	| web      | Special   | on         | true        | not              |
	| mob      | Increased | off        | false       |                  |
	| mob      | Increased | off        | true        |                  |
	| mob      | Increased | on         | false       |                  |
	| mob      | Increased | on         | true        | not              |



