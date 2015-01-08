use dota;

if exists(select 1 from sysobjects where name = 'pending_matches' and type = 'U')
	drop table pending_matches;
	
create table pending_matches (
	match_id bigint,
	lobby_type int,
	public_players int,
	start_time bigint,
	insert_time bigint
);

if exists(select 1 from sysobjects where name = 'pending_players' and type = 'U')
	drop table pending_players;

create table pending_players (
	player_id bigint,
	insert_time bigint
);

insert into pending_players
select 
	mp.player_id,
	min(m.start)
from
	matches m 
	join match_players mp on mp.match_id = m.match_id
where
	mp.player_id > 0	
group by 	
	mp.player_id
	
select count(*) from pending_matches
select count(*) from pending_players
	
98224	
	
select * from players
insert into pending_players select * from #temp_players
select * into #temp_players from pending_players
select player_id, COUNT(*) from pending_players group by player_id having COUNT(*) > 1
select
	count(*)
from 
	pending_players pp
	left outer join match_players mp on mp.player_id = pp.player_id
where 
	mp.player_id is null
	
	
	