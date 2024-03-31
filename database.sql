create table proposta (
    id bigint primary key generated always as identity,
    valor numeric not null
  );

create table eventosOutbox (
    id bigint primary key generated always as identity,
    evento text not null,
    status varchar(15) not null,
    mensagem text not null
  );
