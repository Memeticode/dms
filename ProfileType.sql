
set nocount on;

--select * from dbo.typeEstimateTest
--select * from dbo.typeEstimateTest_SchemaTest

--insert into dbo.typeEstimateTest_SchemaTest
--select * from dbo.typeEstimateTest

--select * from dbo.typeEstimateTest
--select * from dbo.typeEstimateTest_SchemaTest


declare @table_name nvarchar(260) = 'dbo.typeEstimateTest'
, @schema_table_name nvarchar(260) = 'dbo.typeEstimateTest_SchemaTest'

declare
  @col_name nvarchar(128)
, @col_name_str nvarchar(128)
, @vsql_profile_column_type nvarchar(max) = ''
, @vsql_clear_schema_table nvarchar(max) = ''
, @vsql_build_schema_table nvarchar(max) = ''
, @vsql_insert_schema_table nvarchar(max) = ''
, @vsql_query_tables nvarchar(max) = ''
;

declare @schemaPredictionResult table (
	  [id]					int identity(1,1) not null primary key
	, [column_name]			nvarchar(128) not null unique
	, [predicted_type]		nvarchar(128) not null
	, [predicted_nullable]	nvarchar(128) not null
	, [predicted_max_len]	nvarchar(128) null
	, [predicted_fix_len]	nvarchar(128) null
	, [rowcount]			bigint not null
	, [is_null]				bigint not null
	, [is_empty]			bigint not null
	, [total]				bigint not null
	, [max_len_input]		bigint not null
	, [min_len_input]		bigint not null
	, [valid_bool]			bigint not null
	, [valid_int]			bigint not null
	, [valid_bigint]		bigint not null
	, [valid_decimal]		bigint not null
	, [valid_float]			bigint not null
	, [valid_nvarchar]		bigint not null
	, [valid_date]			bigint not null
	, [valid_datetime]		bigint not null
	, [confirm_bool]		bigint not null
	, [date_equals_datetime] bigint not null
	, [decimal_equals_float] bigint not null
)

declare cols cursor static local for 
	select column_name 
	from INFORMATION_SCHEMA.COLUMNS 
	where TABLE_NAME = 'typeEstimateTest'
;
open cols
fetch next from cols into @col_name_str
while @@FETCH_STATUS = 0
begin

set @col_name = quotename(@col_name_str)
;
set @vsql_profile_column_type = ('
select '''+@col_name_str+''' as column_name
	, [predicted_type]
	, [predicted_nullable]
	, [predicted_max_len]
	, [predicted_fix_len]
	, [rowcount]			
	, [is_null]				
	, [is_empty]		
	, [total]				
	, [max_len_input]		
	, [min_len_input]		
	, [valid_bool]			
	, [valid_int]			 
	, [valid_bigint]		 
	, [valid_decimal]		 
	, [valid_float]			 
	, [valid_nvarchar]		 
	, [valid_date]			 
	, [valid_datetime]		 
	, [confirm_bool]		
	, [date_equals_datetime]
	, [decimal_equals_float] 
from (
select *
	, [predicted_fix_len]	
		= case when [predicted_type] <> ''nvarchar'' then null
			when [max_len_input] = [min_len_input] then 1
			else 0 end
	, [predicted_max_len]		
		= case when [predicted_type] <> ''nvarchar'' then null
			when [max_len_input] = [min_len_input] then cast([max_len_input] as nvarchar(10))
			when [max_len_input] < 24  then ''32''
			when [max_len_input] < 48  then ''64''
			when [max_len_input] < 88  then ''128''
			when [max_len_input] < 192 then ''256''
			else ''max'' end
from (
select *
	, [predicted_type] = case 
		when [total] = 0 
			then ''nvarchar''
		when [total] = [valid_datetime] 
			and [total] <> [date_equals_datetime]
			then ''datetime''
		when [total] = [valid_date]
			then ''date''
		when [total] = [valid_bool]	
			and [valid_bool] = [confirm_bool]
			then ''bit''
		when [total] = [valid_decimal]
			and [valid_decimal] <> [valid_bigint]
			then ''decimal''
		when [total] = [valid_float]
			and [valid_float] <> [valid_bigint]
			then ''float''
		when [total] = [valid_int]
			then ''int''
		when [total] = [valid_bigint]
			then ''bigint''
		when [total] = [valid_bool]
			then ''bit''
			else ''nvarchar'' end
	, [predicted_nullable] = case 
		when [rowcount] = [total]
			then ''not null''
			else ''null''
			end
from (
select
	  [rowcount]			= count(1)
	, [is_null]				= sum( iif('+@col_name+' is null, 1, 0)	)
	, [is_empty]			= sum( iif('+@col_name+' = '''', 1, 0)	)
	-- not_null_or_empty
	, [total]				= sum( iif('+@col_name+' is not null and '+@col_name+' <> '''', 1, 0)	)
	, [max_len_input]		= max(len('+@col_name+'))
	, [min_len_input]		= min(len('+@col_name+'))
	, [valid_bool]			= sum( iif(try_cast('+@col_name+' as bit)			 is not null, 1, 0) )
	, [valid_int]			= sum( iif(try_cast('+@col_name+' as int)			 is not null, 1, 0) ) 
	, [valid_bigint]		= sum( iif(try_cast('+@col_name+' as bigint)		 is not null, 1, 0) ) 
	, [valid_decimal]		= sum( iif(try_cast('+@col_name+' as decimal(24,8))	 is not null, 1, 0) ) 
	, [valid_float]			= sum( iif(try_cast('+@col_name+' as float)			 is not null, 1, 0) ) 
	, [valid_nvarchar]		= sum( iif(try_cast('+@col_name+' as nvarchar(max))	 is not null, 1, 0) ) 
	, [valid_date]			= sum( iif(try_cast('+@col_name+' as date)			 is not null, 1, 0) ) 
	, [valid_datetime]		= sum( iif(try_cast('+@col_name+' as datetime)		 is not null, 1, 0) ) 
	, [confirm_bool]		= sum( case 
								when try_cast('+@col_name+' as bit) is not null	
									and try_cast('+@col_name+' as int) is not null
									and try_cast('+@col_name+' as int) in (1,0)
									then 1
									else 0
								end )
	, [date_equals_datetime] = sum( case 
								when try_cast('+@col_name+' as date) is null	
									then 0
								when try_cast(try_cast('+@col_name+' as date) as datetime)
										= try_cast('+@col_name+' as datetime)	
									then 1
									else 0 
								end )
	, [decimal_equals_float] = sum( case 
								when try_cast('+@col_name+' as decimal(32,8)) is not null	
									and try_cast('+@col_name+' as float) is not null	
									and try_cast(try_cast('+@col_name+' as decimal(32,8)) as float)
										= try_cast('+@col_name+' as float)
									then 1
									else 0
								end )
from '+@table_name+'
) a ) a ) a
')
;

insert into @schemaPredictionResult
exec sp_executesql @stmnt = @vsql_profile_column_type
;

fetch next from cols into @col_name_str
end
close cols
deallocate cols




set @vsql_clear_schema_table = 'drop table if exists '+@schema_table_name+';';
exec sp_executesql @stmnt = @vsql_clear_schema_table;


select @vsql_build_schema_table = 
	'create table '+@schema_table_name+' (' + char(10) + '  '
		+ string_agg(
				quotename([column_name])
				+ ' ' 
				+ case when [predicted_type] = 'nvarchar' 
						then [predicted_type] + '('+[predicted_max_len]+')'
					when [predicted_type] = 'decimal' 
						then [predicted_type] + '(24,8)'
						else [predicted_type] end
				+ ' ' 
				+ predicted_nullable
			, char(10)+', ')
		+ char(10)+');'
from @schemaPredictionResult
exec sp_executesql @stmnt = @vsql_build_schema_table;




set @vsql_insert_schema_table = ('
insert into '+@schema_table_name+'
select * from '+@table_name+'
');
exec sp_executesql @stmnt = @vsql_insert_schema_table;



set @vsql_query_tables = ('
select ''Untyped'' as [table];
select * from '+@table_name+';
select ''Typed'' as [table];
select * from '+@schema_table_name+';
');
exec sp_executesql @stmnt = @vsql_query_tables;


