Feature: Paused Merchants for Mob

https://shopgoau.atlassian.net/browse/CPS-1581

@tag1
Scenario Outline: Paused merchants for mob
	Given Feature Flag for Merchant Pause Is '<flagstatus>'
	And one or more '<offertype>' offers are listed with Merchant Paused status as '<pausestatus>'
	When all Offers for '<platform>' of '<offertype>' are requested
	Then the result list does '<containassertion>' contain listed offers

Examples:
	| platform | offertype | flagstatus | pausestatus | containassertion |
	| mob      | Increased | off        | false       |                  |
	| mob      | Increased | off        | true        |                  |
	| mob      | Increased | on         | false       |                  |
	| mob      | Increased | on         | true        | not              |
