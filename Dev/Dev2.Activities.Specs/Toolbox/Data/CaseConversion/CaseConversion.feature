﻿Feature: CaseConversion
	In order to convert the case of words
	As a Warewolf user
	I want a tool that converts words from their current case to a selected case


Scenario: Convert a sentence to uppercase
	Given I have a case convert variable "[[var]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[var]]" to "UPPER"	
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF ROCKS"
	And the execution has "NO" error

Scenario: Convert a sentence to lowercase
	Given I have a case convert variable "[[var]]" with a value of "Warewolf Rocks"	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be "warewolf rocks"
	And the execution has "NO" error

Scenario: Convert a sentence to Sentence
	Given I have a case convert variable "[[var]]" with a value of "WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Sentence"
	When the case conversion tool is executed
	Then the sentence will be "Warewolf rocks"
	And the execution has "NO" error

Scenario: Convert a sentence to Title Case
	Given I have a case convert variable "[[var]]" with a value of "WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be "WAREWOLF Rocks"
	And the execution has "NO" error

Scenario: Convert a sentence starting with a number to UPPER CASE
	Given I have a case convert variable "[[var]]" with a value of "1 Warewolf Rocks"	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF ROCKS"
	And the execution has "NO" error

Scenario: Convert a sentence starting with a number to lower case
	Given I have a case convert variable "[[var]]" with a value of "1 Warewolf Rocks"	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the execution has "NO" error

Scenario: Convert a sentence starting with a number to Sentence case
	Given I have a case convert variable "[[var]]" with a value of "1 WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Sentence"	
	When the case conversion tool is executed
	Then the sentence will be "1 warewolf rocks"
	And the execution has "NO" error

Scenario: Convert a sentence starting with a number to Title Case
	Given I have a case convert variable "[[var]]" with a value of "1 WAREWOLF Rocks"	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be "1 WAREWOLF Rocks"
	And the execution has "NO" error

Scenario: Convert a blank to Title Case
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "Title Case"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error

Scenario: Convert a blank to Sentencecase
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "Sentence"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error

Scenario: Convert a blank to UPPER CASE
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error

Scenario: Convert a blank to lowercase
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "lower"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error

Scenario: Convert a recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | val                 |
	| rs().row | <x id="1">One</x>   |
	| rs().row | <x id="2">two</x>   |
	| rs().row | <x id="3">three</x> |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "rs().row" will be
	| rs       | val                 |
	| rs().row | <X ID="1">ONE</X>   |
	| rs().row | <X ID="2">TWO</X>   |
	| rs().row | <X ID="3">THREE</X> |
	And the execution has "NO" error

Scenario: Convert an empty recordset * to Upper
	Given I have a CaseConversion recordset
	| rs       | row                 |
	And I convert a variable "[[rs(*).row]]" to "UPPER"
	When the case conversion tool is executed
	Then the case convert result for this varibale "rs().row" will be
	| rs       | row                 |
	And the execution has "NO" error

Scenario: Convert a empty sentence starting with a number to upper
	Given I have a case convert variable "[[var]]" with a value of ""	
	And I convert a variable "[[var]]" to "UPPER"
	When the case conversion tool is executed
	Then the sentence will be ""
	And the execution has "NO" error

Scenario: Convert a negative recordset index to uppercase
	Given I have a case convert variable "[[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "UPPER"		
	When the case conversion tool is executed
	Then the execution has "AN" error

Scenario: Convert a negative recordset index to lowercase
	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "lower"		
	When the case conversion tool is executed
	Then the execution has "AN" error

Scenario: Convert a negative recordset index to Sentence
	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "Sentence"		
	When the case conversion tool is executed
	Then the execution has "AN" error

Scenario: Convert a negative recordset index to Title Case
	Given I have a case convert variable "[[my().sentenct]]" with a value of "Warewolf Rocks"
	And I convert a variable "[[my(-1).sentenct]]" to "Title Case"		
	When the case conversion tool is executed
	Then the execution has "AN" error
