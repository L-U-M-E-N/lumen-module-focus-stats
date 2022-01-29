-- Table: public.focus_stats

-- DROP TABLE IF EXISTS public.focus_stats;

CREATE TABLE IF NOT EXISTS public.focus_stats
(
    name character varying COLLATE pg_catalog."default" NOT NULL,
    exe character varying COLLATE pg_catalog."default" NOT NULL,
    date timestamp with time zone NOT NULL,
    duration double precision NOT NULL,
    CONSTRAINT focus_stats_pkey PRIMARY KEY (name, exe, date)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.focus_stats
    OWNER to lumen;
-- Index: focus_stats_date_idx

-- DROP INDEX IF EXISTS public.focus_stats_date_idx;

CREATE INDEX IF NOT EXISTS focus_stats_date_idx
    ON public.focus_stats USING btree
    (date ASC NULLS LAST, duration ASC NULLS LAST)
    TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.focus_stats
    CLUSTER ON focus_stats_date_idx;


-- Table: public.focus_stats_tags

-- DROP TABLE IF EXISTS public.focus_stats_tags;

CREATE TABLE IF NOT EXISTS public.focus_stats_tags
(
    name character varying COLLATE pg_catalog."default" NOT NULL,
    exe character varying COLLATE pg_catalog."default" NOT NULL,
    tag character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk PRIMARY KEY (name, exe, tag)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.focus_stats_tags
    OWNER to lumen;
-- Index: focus_stats_name-exe_idx

-- DROP INDEX IF EXISTS public."focus_stats_name-exe_idx";

CREATE INDEX IF NOT EXISTS "focus_stats_name-exe_idx"
    ON public.focus_stats_tags USING btree
    (name COLLATE pg_catalog."default" ASC NULLS LAST, exe COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;