Declare @stringXML as nvarchar(max)
Set @stringXML = ''
exec sp_AE_GenerateXML {0}, @stringXML output
Set @stringXML = REPLACE(@stringXML, ' ' , '¿')
Declare @table as table (Value nvarchar(256))
Declare @StringLen as int
Declare @Cursor as int
set @Cursor = 0
If @stringXML <> ''
Begin
set @StringLen = (select LEN(@stringXML))
	while @Cursor <= @StringLen
	Begin
		insert into @table  select substring(@stringXML	, @Cursor, 100)
		Set @Cursor = @Cursor + 100
		
	End;	
End
select Value from @table
