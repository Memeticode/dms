
drop table if exists dbo.typeEstimateTest
;
create table dbo.typeEstimateTest (
	  colBoolBit nvarchar(128) null	-- bool
	, colBoolTF nvarchar(128) null	-- bool
	, colStrLen3 nvarchar(128) null	-- bool
	, colInt nvarchar(128) null	-- int
	, colBigint nvarchar(128) null	-- bigint
	, colDecimal nvarchar(128) null	-- decimal
	, colFloat nvarchar(128) null	-- float
	, colStr128 nvarchar(128) null	-- nvarchar(128)
	, colStr10 nvarchar(128) null	-- nvarchar(10)
	, colDate nvarchar(128) null	-- date
	, colDatetime nvarchar(128) null	-- datetime
)
;
insert into dbo.typeEstimateTest (
	  colBoolBit
	, colBoolTF
	, colStrLen3
	, colInt
	, colBigint
	, colDecimal
	, colFloat
	, colStr128
	, colStr10
	, colDate
	, colDatetime
)
values 
( 1 ,'true'  ,'yes' ,1	,1				,'1.23'		  ,'1.23457e+029'		,'123'			,'1234567890'	,'2022-07-10'	,'2022-07-10 17:57:11.053'	)
,(0 ,'false' ,'no'  ,12	,12345678919	,'19.21'	  ,'1.23457e+021'		,'abcdefsgfa'	    ,'123456789A'	,'1999-01-10'	,'1999-01-10 12:12:12'	)
,(1 ,'TRUE'  ,'yes' ,99	,99945678919	,'9.17'		  ,'9.56829e+017'		,'Hello, string!' ,'ABCDEFGHIJ'	,'2001-12-31'	,'2016-01-10 12:12:12'		)
,(0 ,'FALSE' ,'no'  ,12	,-12345			,'100287.323' ,'-6.90256e+029'		,'    '		    ,'JABCDEFGHI'	,'2010-06-06'	,'1977-11-19 00:12:12.123'	)
,(1 ,'falsE' ,'no'  ,99	,-99945678919	,'9.5682'	  ,'9.56824e+017'		,'ABCPOL_90'	    ,'IJABCDEFGH'	,'2001-12-31'	,'2009-01-10 00:00:00.999'	)
;

