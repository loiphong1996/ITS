create table sysdiagrams
(
  name         sysname not null,
  principal_id int     not null,
  diagram_id   int identity,
  version      int,
  definition   varbinary(max),
  primary key (diagram_id),
  constraint UK_principal_name
  unique (principal_id, name)
)
go

create table User
(
  Name      nvarchar(255) not null,
  Email     nvarchar(320) not null,
  Password  nvarchar(255) not null,
  Phone     nvarchar(50),
  Birthdate date,
  Avatar    nvarchar(255),
  Address   nvarchar(max),
  IsAdmin   bit           not null,
  IsActive  bit           not null,
  Id        int           not null,
  constraint User_Id_pk
  primary key (Id)
)
go

create table Question
(
  Id       int identity,
  Text     nvarchar(max),
  Category nvarchar(255),
  primary key (Id)
)
go

create table Answer
(
  Id         int not null,
  Text       nvarchar(max),
  QuestionId int,
  primary key (Id),
  constraint Answer_Question_Id_fk
  foreign key (QuestionId) references Question
)
go

create table Tag
(
  Id       int identity,
  Name     nvarchar(255),
  Category nvarchar(255),
  primary key (Id)
)
go

create table Area
(
  Id          int identity,
  Name        nvarchar(max),
  Description nvarchar(max),
  Country     nvarchar(max),
  primary key (Id)
)
go

create table Location
(
  Id          int identity,
  Name        nvarchar(255),
  Address     nvarchar(max),
  Description nvarchar(max),
  Longitude   float,
  Latitude    float,
  Website     nvarchar(max),
  Email       nvarchar(320),
  IsVerified  bit,
  IsClosed    bit,
  IsDeleted   bit,
  OwnerId     int,
  AreaId      int,
  primary key (Id),
  constraint Location_User_Id_fk
  foreign key (OwnerId) references User,
  constraint Location_Area_Id_fk
  foreign key (AreaId) references Area
)
go

create table ChangeRequest
(
  Id            int not null,
  Name          nvarchar(255),
  Description   nvarchar(max),
  Website       nvarchar(255),
  Phone         nvarchar(50),
  Email         nvarchar(300),
  BusinessHours nvarchar(max),
  Tags          nvarchar(max),
  Status        int,
  SenderId      int,
  LocationId    int,
  primary key (Id),
  constraint ChangeRequest_User_Id_fk
  foreign key (SenderId) references User,
  constraint ChangeRequest_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table ClaimOwnerRequest
(
  Id          int identity,
  Description nvarchar(max),
  Status      int,
  SenderId    int,
  LocationId  int,
  primary key (Id),
  constraint ClaimOwnerRequest_User_Id_fk
  foreign key (SenderId) references User,
  constraint ClaimOwnerRequest_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table BusinessHours
(
  Id         int identity,
  Day        int,
  OpenTime   time,
  CloseTime  time,
  LocationId int,
  primary key (Id),
  constraint BusinessHours_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table Group
(
  Id      int identity,
  Name    nvarchar(max),
  OwnerId int,
  primary key (Id),
  constraint Group_User_Id_fk
  foreign key (OwnerId) references User
)
go

create table GroupInvitation
(
  Id      int identity,
  Message nvarchar(max),
  Status  int,
  UserId  int,
  GroupId int,
  primary key (Id),
  constraint GroupInvitation_User_Id_fk
  foreign key (UserId) references User,
  constraint GroupInvitation_Group_Id_fk
  foreign key (GroupId) references Group
)
go

create unique index GroupInvitation_Id_uindex
  on GroupInvitation (Id)
go

create table Plan
(
  Id        int identity,
  Name      nvarchar(max),
  StartDate date,
  EndDate   date,
  IsPublic  bit,
  GroupId   int,
  CreatorId int,
  primary key (Id),
  constraint Plan_Group_Id_fk
  foreign key (GroupId) references Group,
  constraint Plan_User_Id_fk
  foreign key (CreatorId) references User
)
go

create table Note
(
  Id      int identity,
  Title   nvarchar(255),
  Text    nvarchar(max),
  PlanDay int,
  Index int,
  PlanId  int,
  primary key (Id),
  constraint Note_Plan_Id_fk
  foreign key (PlanId) references Plan
)
go

create table PlanLocation
(
  Id         int identity,
  PlanDay    int,
  Index int,
  Comment    nvarchar(max),
  PlanId     int,
  LocationId int,
  primary key (Id),
  constraint PlanLocation_Plan_Id_fk
  foreign key (PlanId) references Plan,
  constraint PlanLocation_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table LocationSuggestion
(
  Id         int identity,
  Status     int,
  Comment    nvarchar(max),
  SenderId   int,
  PlanId     int,
  LocationId int,
  primary key (Id),
  constraint LocationSuggestion_User_Id_fk
  foreign key (SenderId) references User,
  constraint LocationSuggestion_Plan_Id_fk
  foreign key (PlanId) references Plan,
  constraint LocationSuggestion_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table PlanVote
(
  Id      int identity,
  VoterId int,
  PlanId  int,
  primary key (Id),
  constraint PlanVote_User_Id_fk
  foreign key (VoterId) references User,
  constraint PlanVote_Plan_Id_fk
  foreign key (PlanId) references Plan
)
go

create table Review
(
  Id          int identity,
  Title       nvarchar(max),
  Rating      float,
  Description nvarchar(max),
  AuthorId    int,
  LocationId  int,
  primary key (Id),
  constraint Review_User_Id_fk
  foreign key (AuthorId) references User,
  constraint Review_Location_Id_fk
  foreign key (LocationId) references Location
)
go

create table Report
(
  Id         int identity,
  Text       nvarchar(max),
  ReviewId   int,
  ReporterId int,
  primary key (Id),
  constraint Report_Review_Id_fk
  foreign key (ReviewId) references Review,
  constraint Report_User_Id_fk
  foreign key (ReporterId) references User
)
go

create table Photo
(
  Id         int identity,
  Name       nvarchar(max),
  Path       nvarchar(255),
  AuthorId   int,
  ReviewId   int,
  LocationId int,
  AreaId     int,
  primary key (Id),
  constraint Photo_User_Id_fk
  foreign key (AuthorId) references User,
  constraint Photo_Review_Id_fk
  foreign key (ReviewId) references Review,
  constraint Photo_Location_Id_fk
  foreign key (LocationId) references Location,
  constraint Photo_Area_Id_fk
  foreign key (AreaId) references Area
)
go

create table ReviewVote
(
  Id       int identity,
  VoterId  int,
  ReviewId int,
  primary key (Id),
  constraint ReviewVote_User_Id_fk
  foreign key (VoterId) references User,
  constraint ReviewVote_Review_Id_fk
  foreign key (ReviewId) references Review
)
go

create table UserGroup
(
  UserId  int,
  GroupId int,
  constraint UserGroup_User_Id_fk
  foreign key (UserId) references User,
  constraint UserGroup_Group_Id_fk
  foreign key (GroupId) references Group
)
go

create table LocationTag
(
  Id         int identity,
  LocationId int,
  TagId      int,
  primary key (Id),
  constraint LocationTag_Location_Id_fk
  foreign key (LocationId) references Location,
  constraint LocationTag_Tag_Id_fk
  foreign key (TagId) references Tag
)
go

create table AnswerTag
(
  Id       int not null,
  AnswerId int,
  TagId    int,
  constraint AnswerTag_Id_pk
  primary key (Id),
  constraint AnswerTag_Answer_Id_fk
  foreign key (AnswerId) references Answer,
  constraint AnswerTag_Tag_Id_fk
  foreign key (TagId) references Tag
)
go

create table QuestionArea
(
  QuestionId int,
  AreaId     int,
  Index int,
  constraint QuestionArea_Question_Id_fk
  foreign key (QuestionId) references Question,
  constraint QuestionArea_Area_Id_fk
  foreign key (AreaId) references Area
)
go


CREATE PROCEDURE dbo.sp_upgraddiagrams
AS
  BEGIN
    IF OBJECT_ID(N'dbo.sysdiagrams') IS NOT NULL
      return 0;

    CREATE TABLE dbo.sysdiagrams
    (
      name         sysname NOT NULL,
      principal_id int     NOT NULL, -- we may change it to varbinary(85)
      diagram_id   int PRIMARY KEY IDENTITY,
      version      int,

      definition   varbinary(max)
        CONSTRAINT UK_principal_name UNIQUE
          (
            principal_id,
            name
          )
    );


    /* Add this if we need to have some form of extended properties for diagrams */
    /*
    IF OBJECT_ID(N'dbo.sysdiagram_properties') IS NULL
    BEGIN
      CREATE TABLE dbo.sysdiagram_properties
      (
        diagram_id int,
        name sysname,
        value varbinary(max) NOT NULL
      )
    END
    */

    IF OBJECT_ID(N'dbo.dtproperties') IS NOT NULL
      begin
        insert into dbo.sysdiagrams
        (
          [name],
          [principal_id],
          [version],
          [definition]
        )
          select
            convert(sysname, dgnm.[uvalue]),
            DATABASE_PRINCIPAL_ID(N'dbo'),
            -- will change to the sid of sa
            0,
            -- zero for old format, dgdef.[version],
            dgdef.[lvalue]
          from dbo.[dtproperties] dgnm
            inner join dbo.[dtproperties] dggd
              on dggd.[property] = 'DtgSchemaGUID' and dggd.[objectid] = dgnm.[objectid]
            inner join dbo.[dtproperties] dgdef
              on dgdef.[property] = 'DtgSchemaDATA' and dgdef.[objectid] = dgnm.[objectid]

          where dgnm.[property] = 'DtgSchemaNAME' and dggd.[uvalue] like N'_EA3E6268-D998-11CE-9454-00AA00A3F36E_'
        return 2;
      end
    return 1;
  END


go


CREATE PROCEDURE dbo.sp_helpdiagrams
  (
    @diagramname sysname = NULL,
    @owner_id    int = NULL
  )
  WITH EXECUTE AS N'dbo'
AS
  BEGIN
    DECLARE @user sysname
    DECLARE @dboLogin bit
    EXECUTE AS CALLER;
    SET @user = USER_NAME();
    SET @dboLogin = CONVERT(bit, IS_MEMBER('db_owner'));
    REVERT;
    SELECT
        [Database] = DB_NAME(),
        [Name] = name,
        [ID] = diagram_id,
        [Owner] = USER_NAME(principal_id),
        [OwnerID] = principal_id
    FROM
      sysdiagrams
    WHERE
      (@dboLogin = 1 OR USER_NAME(principal_id) = @user) AND
      (@diagramname IS NULL OR name = @diagramname) AND
      (@owner_id IS NULL OR principal_id = @owner_id)
    ORDER BY
      4, 5, 1
  END


go


CREATE PROCEDURE dbo.sp_helpdiagramdefinition
  (
    @diagramname sysname,
    @owner_id    int = null
  )
  WITH EXECUTE AS N'dbo'
AS
  BEGIN
    set nocount on

    declare @theId int
    declare @IsDbo int
    declare @DiagId int
    declare @UIDFound int

    if (@diagramname is null)
      begin
        RAISERROR (N'E_INVALIDARG', 16, 1);
        return -1
      end

    execute as caller;
    select @theId = DATABASE_PRINCIPAL_ID();
    select @IsDbo = IS_MEMBER(N'db_owner');
    if (@owner_id is null)
      select @owner_id = @theId;
    revert;

    select
      @DiagId = diagram_id,
      @UIDFound = principal_id
    from dbo.sysdiagrams
    where principal_id = @owner_id and name = @diagramname;
    if (@DiagId IS NULL or (@IsDbo = 0 and @UIDFound <> @theId))
      begin
        RAISERROR ('Diagram does not exist or you do not have permission.', 16, 1);
        return -3
      end

    select
      version,
      definition
    FROM dbo.sysdiagrams
    where diagram_id = @DiagId;
    return 0
  END


go


CREATE PROCEDURE dbo.sp_creatediagram
  (
    @diagramname sysname,
    @owner_id    int = null,
    @version     int,
    @definition  varbinary(max)
  )
  WITH EXECUTE AS 'dbo'
AS
  BEGIN
    set nocount on

    declare @theId int
    declare @retval int
    declare @IsDbo int
    declare @userName sysname
    if (@version is null or @diagramname is null)
      begin
        RAISERROR (N'E_INVALIDARG', 16, 1);
        return -1
      end

    execute as caller;
    select @theId = DATABASE_PRINCIPAL_ID();
    select @IsDbo = IS_MEMBER(N'db_owner');
    revert;

    if @owner_id is null
      begin
        select @owner_id = @theId;
      end
    else
      begin
        if @theId <> @owner_id
          begin
            if @IsDbo = 0
              begin
                RAISERROR (N'E_INVALIDARG', 16, 1);
                return -1
              end
            select @theId = @owner_id
          end
      end
    -- next 2 line only for test, will be removed after define name unique
    if EXISTS(select diagram_id
              from dbo.sysdiagrams
              where principal_id = @theId and name = @diagramname)
      begin
        RAISERROR ('The name is already used.', 16, 1);
        return -2
      end

    insert into dbo.sysdiagrams (name, principal_id, version, definition)
    VALUES (@diagramname, @theId, @version, @definition);

    select @retval = @@IDENTITY
    return @retval
  END


go


CREATE PROCEDURE dbo.sp_renamediagram
  (
    @diagramname     sysname,
    @owner_id        int = null,
    @new_diagramname sysname

  )
  WITH EXECUTE AS 'dbo'
AS
  BEGIN
    set nocount on
    declare @theId int
    declare @IsDbo int

    declare @UIDFound int
    declare @DiagId int
    declare @DiagIdTarg int
    declare @u_name sysname
    if ((@diagramname is null) or (@new_diagramname is null))
      begin
        RAISERROR ('Invalid value', 16, 1);
        return -1
      end

    EXECUTE AS CALLER;
    select @theId = DATABASE_PRINCIPAL_ID();
    select @IsDbo = IS_MEMBER(N'db_owner');
    if (@owner_id is null)
      select @owner_id = @theId;
    REVERT;

    select @u_name = USER_NAME(@owner_id)

    select
      @DiagId = diagram_id,
      @UIDFound = principal_id
    from dbo.sysdiagrams
    where principal_id = @owner_id and name = @diagramname
    if (@DiagId IS NULL or (@IsDbo = 0 and @UIDFound <> @theId))
      begin
        RAISERROR ('Diagram does not exist or you do not have permission.', 16, 1)
        return -3
      end

    -- if((@u_name is not null) and (@new_diagramname = @diagramname))	-- nothing will change
    --	return 0;

    if (@u_name is null)
      select @DiagIdTarg = diagram_id
      from dbo.sysdiagrams
      where principal_id = @theId and name = @new_diagramname
    else
      select @DiagIdTarg = diagram_id
      from dbo.sysdiagrams
      where principal_id = @owner_id and name = @new_diagramname

    if ((@DiagIdTarg is not null) and @DiagId <> @DiagIdTarg)
      begin
        RAISERROR ('The name is already used.', 16, 1);
        return -2
      end

    if (@u_name is null)
      update dbo.sysdiagrams
      set [name] = @new_diagramname, principal_id = @theId
      where diagram_id = @DiagId
    else
      update dbo.sysdiagrams
      set [name] = @new_diagramname
      where diagram_id = @DiagId
    return 0
  END


go


CREATE PROCEDURE dbo.sp_alterdiagram
  (
    @diagramname sysname,
    @owner_id    int = null,
    @version     int,
    @definition  varbinary(max)
  )
  WITH EXECUTE AS 'dbo'
AS
  BEGIN
    set nocount on

    declare @theId int
    declare @retval int
    declare @IsDbo int

    declare @UIDFound int
    declare @DiagId int
    declare @ShouldChangeUID int

    if (@diagramname is null)
      begin
        RAISERROR ('Invalid ARG', 16, 1)
        return -1
      end

    execute as caller;
    select @theId = DATABASE_PRINCIPAL_ID();
    select @IsDbo = IS_MEMBER(N'db_owner');
    if (@owner_id is null)
      select @owner_id = @theId;
    revert;

    select @ShouldChangeUID = 0
    select
      @DiagId = diagram_id,
      @UIDFound = principal_id
    from dbo.sysdiagrams
    where principal_id = @owner_id and name = @diagramname

    if (@DiagId IS NULL or (@IsDbo = 0 and @theId <> @UIDFound))
      begin
        RAISERROR ('Diagram does not exist or you do not have permission.', 16, 1);
        return -3
      end

    if (@IsDbo <> 0)
      begin
        if (@UIDFound is null or USER_NAME(@UIDFound) is null) -- invalid principal_id
          begin
            select @ShouldChangeUID = 1;
          end
      end

    -- update dds data			
    update dbo.sysdiagrams
    set definition = @definition
    where diagram_id = @DiagId;

    -- change owner
    if (@ShouldChangeUID = 1)
      update dbo.sysdiagrams
      set principal_id = @theId
      where diagram_id = @DiagId;

    -- update dds version
    if (@version is not null)
      update dbo.sysdiagrams
      set version = @version
      where diagram_id = @DiagId;

    return 0
  END


go


CREATE PROCEDURE dbo.sp_dropdiagram
  (
    @diagramname sysname,
    @owner_id    int = null
  )
  WITH EXECUTE AS 'dbo'
AS
  BEGIN
    set nocount on
    declare @theId int
    declare @IsDbo int

    declare @UIDFound int
    declare @DiagId int

    if (@diagramname is null)
      begin
        RAISERROR ('Invalid value', 16, 1);
        return -1
      end

    EXECUTE AS CALLER;
    select @theId = DATABASE_PRINCIPAL_ID();
    select @IsDbo = IS_MEMBER(N'db_owner');
    if (@owner_id is null)
      select @owner_id = @theId;
    REVERT;

    select
      @DiagId = diagram_id,
      @UIDFound = principal_id
    from dbo.sysdiagrams
    where principal_id = @owner_id and name = @diagramname
    if (@DiagId IS NULL or (@IsDbo = 0 and @UIDFound <> @theId))
      begin
        RAISERROR ('Diagram does not exist or you do not have permission.', 16, 1)
        return -3
      end

    delete from dbo.sysdiagrams
    where diagram_id = @DiagId;

    return 0;
  END


go


CREATE FUNCTION dbo.fn_diagramobjects()
  RETURNS int
  WITH EXECUTE AS N'dbo'
AS
  BEGIN
    declare @id_upgraddiagrams int
    declare @id_sysdiagrams int
    declare @id_helpdiagrams int
    declare @id_helpdiagramdefinition int
    declare @id_creatediagram int
    declare @id_renamediagram int
    declare @id_alterdiagram int
    declare @id_dropdiagram int
    declare @InstalledObjects int

    select @InstalledObjects = 0

    select
      @id_upgraddiagrams = object_id(N'dbo.sp_upgraddiagrams'),
      @id_sysdiagrams = object_id(N'dbo.sysdiagrams'),
      @id_helpdiagrams = object_id(N'dbo.sp_helpdiagrams'),
      @id_helpdiagramdefinition = object_id(N'dbo.sp_helpdiagramdefinition'),
      @id_creatediagram = object_id(N'dbo.sp_creatediagram'),
      @id_renamediagram = object_id(N'dbo.sp_renamediagram'),
      @id_alterdiagram = object_id(N'dbo.sp_alterdiagram'),
      @id_dropdiagram = object_id(N'dbo.sp_dropdiagram')

    if @id_upgraddiagrams is not null
      select @InstalledObjects = @InstalledObjects + 1
    if @id_sysdiagrams is not null
      select @InstalledObjects = @InstalledObjects + 2
    if @id_helpdiagrams is not null
      select @InstalledObjects = @InstalledObjects + 4
    if @id_helpdiagramdefinition is not null
      select @InstalledObjects = @InstalledObjects + 8
    if @id_creatediagram is not null
      select @InstalledObjects = @InstalledObjects + 16
    if @id_renamediagram is not null
      select @InstalledObjects = @InstalledObjects + 32
    if @id_alterdiagram is not null
      select @InstalledObjects = @InstalledObjects + 64
    if @id_dropdiagram is not null
      select @InstalledObjects = @InstalledObjects + 128

    return @InstalledObjects
  END


go

