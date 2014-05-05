﻿Feature: Count
	In order to count records
	As a Warewolf user
	I want a tool that takes a record set counts it

Scenario: Count a number of records in a recordset with 3 rows
	Given I have a recordset with this shape
	| [[rs]]   |   |
	| rs().row | 1 |
	| rs().row | 2 |
	| rs().row | 3 |
	And count on record "[[rs()]]"	
	When the count tool is executed
	Then the result count should be 3
	And the execution has "NO" error
	And the debug inputs as 
	| Recordset         |
	| [[rs(1).row]] = 1 |
	| [[rs(2).row]] = 2 |
	| [[rs(3).row]] = 3 |
	And the debug output as 
	|                 |
	| [[result]] = 3 |


Scenario: Count a number of records in a recordset with 8 rows
	Given I have a recordset with this shape
	| rs       |   |
	| rs().row | 1 |
	| rs().row | 2 |
	| rs().row | 3 |
	| rs().row | 4 |
	| rs().row | 5 |
	| rs().row | 6 |
	| rs().row | 7 |
	| rs().row | 8 |
	And count on record "[[rs()]]"	
	When the count tool is executed
	Then the result count should be 8
	And the execution has "NO" error
	And the debug inputs as  
	| Recordset          |
	| [[rs(1).row]] =  1 |
	| [[rs(2).row]] =  2 |
	| [[rs(3).row]] =  3 |
	| [[rs(4).row]] =  4 |
	| [[rs(5).row]] =  5 |
	| [[rs(6).row]] =  6 |
	| [[rs(7).row]] =  7 |
	| [[rs(8).row]] =  8 |
	And the debug output as 
	|                 |
	| [[result]] = 8 |

Scenario: Count a number of records in a recordset with 0 rows
	Given I have a recordset with this shape
	| rs      |
	And count on record "[[rs()]]"	
	When the count tool is executed
	Then the result count should be 0
	And the execution has "NO" error
	And the debug inputs as  
	| Recordset  |
	| [[rs()]] = |
	And the debug output as 
	|                |
	| [[result]] = 0 |


#Scenario: Count record with invalid variables
#	Given I have a recordset with this shape
#	| rs       |   |
#	| rs().row | 1 |
#	| rs().row | 2 |
#	| rs().row | 3 |
#	| rs().row | 4 |
#	| rs().row | 5 |
#	| rs().row | 6 |
#	| rs().row | 7 |
#	| rs().row | 8 |
#	And count on record "[[rs().#$]]"	
#	When the count tool is executed
#	Then the result count should be 8
#	And the execution has "AN" error
#	And the debug inputs as  
#	| Recordset          |
#	And the debug output as 
#	|              |
#	| [[result]] = |
#
#Scenario: Count only one column record
#	Given I have a recordset with this shape
#	| rs       |   |
#	| rs().row | 1 |
#	| rs().row | 2 |
#	| rs().row | 3 |
#	| rs().row | 4 |
#	| rs().row | 5 |
#	| rs().row | 6 |
#	| rs().row | 7 |
#	| rs().row | 8 |
#	And count on record "[[rs(*).row]]"	
#	When the count tool is executed
#	Then the result count should be 8
#	And the execution has "AN" error
#	And the debug inputs as  
#	| Recordset          |
#	And the debug output as 
#	|              |
#	| [[result]] = |

#Scenario: Count only one coloumn record
#	Given I have a recordset with this shape
#	| rs       |   |
#	| rs().row | 1 |
#	| rs().row | 2 |
#	| rs().row | 3 |
#	| rs().row | 4 |
#	| fs().row | 5 |
#	| fs().row | 6 |
#	| fs().row | 7 |
#	| fs().row | 8 |
#	And count on record "[[rs().row]],[[fs().row]]"	
#	When the count tool is executed
#	Then the result count should be 8
#	And the execution has "AN" error
#	And the debug inputs as  
#	| Recordset          |
#	And the debug output as 
#	|              |
#	| [[result]] = |

#Scenario: Count only one coloumn record
#	Given I have a recordset with this shape
#	| rs       |   |
#	| rs().row | 1 |
#	| rs().row | 2 |
#	| rs().row | 3 |
#	| rs().row | 4 |
#	| fs().row | 5 |
#	| fs().row | 6 |
#	| fs().row | 7 |
#	| fs().row | 8 |
#	And count on record "[[rs().row]],[[fs().row]]"	
#	When the count tool is executed
#	Then the result count should be 8
#	And the execution has "AN" error
#	And the debug inputs as  
#	| Recordset          |
#	And the debug output as 
#	|              |
#	| [[result]] = |

#Scenario: Count a number of records when two recordsets are defined.
	#Given I have a recordset with this shape
	#| rs       |
	#| rs().row |
	#| fs().row |
	#| rs().row |	
	#| rs().row |
	#| fs().row |
	#| rs().row |	
	#| fs().row |
	#| rs().row |	
	#When the count tool is executed
	#Then the result count should be 5
	#And the execution has "NO" error
	#And the debug inputs as  
	#| Recordset        |
	#| [[rs(1).row]] =  |
	#| [[rs(2).row]] =  |
	#| [[rs(3).row]] =  |
	#| [[rs(4).row]] =  |
	#| [[rs(5).row]] =  |
	#And the debug output as 
	#| [[result]] = 5|

#Scenario: Executing Count with two variables in result field
#	Given I have a recordset with this shape
#	| rs        |   |
#	| rs(1).row | 1 |
#	| rs(2).row | 2 |
#	| rs(3).row | 3 |
#	| rs(4).row | 4 |
#	And count on record "[[rs()]]"
#    And result varaible as "[[rs().r]][[a]]"	
#	When the count tool is executed
#	Then the result count should be 8
#	And the execution has "AN" error
#	And the debug inputs as  
#	| Recordset          |
#	And the debug output as 
#	|                   |
#	| [[rs().r]][[a]] = |
