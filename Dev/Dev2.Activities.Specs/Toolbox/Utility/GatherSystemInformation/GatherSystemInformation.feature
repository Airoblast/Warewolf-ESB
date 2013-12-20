﻿Feature: GatherSystemInformation
	In order to use system information
	As a warewolf user
	I want a tool that I retrieve system info


Scenario: Assign a system operating system into a scalar
	Given I have a variable "[[testvar]]" and I selected "OperatingSystem"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system service pack into a scalar
	Given I have a variable "[[testvar]]" and I selected "ServicePack"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system OS Bit Value into a scalar
	Given I have a variable "[[testvar]]" and I selected "OSBitValue"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "Int32"
	And the execution has "NO" error

Scenario: Assign a system date time into a scalar
	Given I have a variable "[[testvar]]" and I selected "FullDateTime"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "DateTime"
	And the execution has "NO" error

Scenario: Assign a system Date Time Format into a scalar
	Given I have a variable "[[testvar]]" and I selected "DateTimeFormat"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Disk Available into a scalar
	Given I have a variable "[[testvar]]" and I selected "DiskAvailable"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Disk Total into a scalar
	Given I have a variable "[[testvar]]" and I selected "DiskTotal"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Physical Memory Available into a scalar
	Given I have a variable "[[testvar]]" and I selected "PhysicalMemoryAvailable"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "Int32"
	And the execution has "NO" error

Scenario: Assign a system Physical Memory Total into a scalar
	Given I have a variable "[[testvar]]" and I selected "PhysicalMemoryTotal"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "Int32"
	And the execution has "NO" error

Scenario: Assign a system CPU Available into a scalar
	Given I have a variable "[[testvar]]" and I selected "CPUAvailable"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system CPU Total into a scalar
	Given I have a variable "[[testvar]]" and I selected "CPUTotal"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Language into a scalar
	Given I have a variable "[[testvar]]" and I selected "Language"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Region into a scalar
	Given I have a variable "[[testvar]]" and I selected "Region"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system User Roles into a scalar
	Given I have a variable "[[testvar]]" and I selected "UserRoles"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system User Name into a scalar
	Given I have a variable "[[testvar]]" and I selected "UserName"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Domain into a scalar
	Given I have a variable "[[testvar]]" and I selected "Domain"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Number Of Warewolf Agents into a scalar
	Given I have a variable "[[testvar]]" and I selected "NumberOfWarewolfAgents"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[testvar]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign User Roles into a recordset
	Given I have a variable "[[my().roles]]" and I selected "UserRoles"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[my(2).roles]]" is a valid "String"
	And the execution has "NO" error

Scenario: Assign a system Domain into a negative recordset index
	Given I have a variable "[[rec(-1).set]]" and I selected "Domain"	
	When the gather system infomartion tool is executed
	Then the execution has "AN" error

#This scenario requires the machine the test runs on to have more than 1 drive. You can map a network drive if it only has 1 logical.
Scenario: Assign Disk Total into a recordset
	Given I have a variable "[[my().disks]]" and I selected "DiskTotal"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[my(2).disks]]" is a valid "String"
	And the execution has "NO" error

#This scenario requires the machine the test runs on to have more than 1 drive. You can map a network drive if it only has 1 logical.
Scenario: Assign Disk Available into a recordset
	Given I have a variable "[[my().disks]]" and I selected "DiskAvailable"	
	When the gather system infomartion tool is executed
	Then the value of the variable "[[my(2).disks]]" is a valid "String"
	And the execution has "NO" error
