--
-- PostgreSQL database dump
--

\restrict mBZvYuilaThNNTjo4j92xxqp2Xbs9aXHYilWWKtJcLLIjCRyWz28OtrrfbO4ega

-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.3

-- Started on 2026-06-20 12:36:59

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 5 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: azure_pg_admin
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO azure_pg_admin;

--
-- TOC entry 945 (class 1247 OID 24926)
-- Name: AIReviewStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."AIReviewStatus" AS ENUM (
    'Suggested',
    'Accepted',
    'Rejected',
    'Adjusted'
);


ALTER TYPE public."AIReviewStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 942 (class 1247 OID 24916)
-- Name: AlertSeverity; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."AlertSeverity" AS ENUM (
    'Low',
    'Medium',
    'High',
    'Critical'
);


ALTER TYPE public."AlertSeverity" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 927 (class 1247 OID 24850)
-- Name: BatchStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."BatchStatus" AS ENUM (
    'Planned',
    'Growing',
    'Harvested',
    'Discarded',
    'Completed'
);


ALTER TYPE public."BatchStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 921 (class 1247 OID 24828)
-- Name: DesignType; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."DesignType" AS ENUM (
    'CompletelyRandomized',
    'RandomizedCompleteBlock',
    'Factorial',
    'Observational',
    'Other'
);


ALTER TYPE public."DesignType" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 948 (class 1247 OID 24936)
-- Name: DocumentStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."DocumentStatus" AS ENUM (
    'Draft',
    'Indexed',
    'Archived'
);


ALTER TYPE public."DocumentStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 915 (class 1247 OID 24808)
-- Name: ExperimentStageType; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."ExperimentStageType" AS ENUM (
    'Nursery',
    'Care',
    'Growth',
    'Harvest',
    'Evaluation',
    'Other'
);


ALTER TYPE public."ExperimentStageType" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 912 (class 1247 OID 24796)
-- Name: ExperimentStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."ExperimentStatus" AS ENUM (
    'Draft',
    'Approved',
    'Active',
    'Completed',
    'Cancelled'
);


ALTER TYPE public."ExperimentStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 918 (class 1247 OID 24822)
-- Name: GroupType; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."GroupType" AS ENUM (
    'Control',
    'Treatment'
);


ALTER TYPE public."GroupType" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 924 (class 1247 OID 24840)
-- Name: LocationStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."LocationStatus" AS ENUM (
    'Available',
    'InUse',
    'Maintenance',
    'Unavailable'
);


ALTER TYPE public."LocationStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 906 (class 1247 OID 24780)
-- Name: RequestStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."RequestStatus" AS ENUM (
    'Pending',
    'Approved',
    'Rejected',
    'Cancelled'
);


ALTER TYPE public."RequestStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 909 (class 1247 OID 24790)
-- Name: ReviewResult; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."ReviewResult" AS ENUM (
    'Approved',
    'Rejected'
);


ALTER TYPE public."ReviewResult" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 939 (class 1247 OID 24902)
-- Name: SensorType; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."SensorType" AS ENUM (
    'Temperature',
    'Humidity',
    'SoilMoisture',
    'Light',
    'PH',
    'Other'
);


ALTER TYPE public."SensorType" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 936 (class 1247 OID 24890)
-- Name: TaskAssignmentStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."TaskAssignmentStatus" AS ENUM (
    'Assigned',
    'Reassigned',
    'Resigned',
    'Completed',
    'Cancelled'
);


ALTER TYPE public."TaskAssignmentStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 933 (class 1247 OID 24878)
-- Name: TaskStatus; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."TaskStatus" AS ENUM (
    'Pending',
    'InProgress',
    'Completed',
    'Overdue',
    'Cancelled'
);


ALTER TYPE public."TaskStatus" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 930 (class 1247 OID 24862)
-- Name: TaskType; Type: TYPE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TYPE public."TaskType" AS ENUM (
    'Planting',
    'Watering',
    'Fertilizing',
    'Observation',
    'Inspection',
    'Harvest',
    'Other'
);


ALTER TYPE public."TaskType" OWNER TO "SEP490_Smartfarm_DB";

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 248 (class 1259 OID 25383)
-- Name: AITaskAssignmentSuggestions; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."AITaskAssignmentSuggestions" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "TaskId" uuid NOT NULL,
    "SuggestedUserId" uuid NOT NULL,
    "MatchScore" numeric(5,4),
    "Reason" text,
    "ReviewStatus" public."AIReviewStatus" DEFAULT 'Suggested'::public."AIReviewStatus" NOT NULL,
    "ReviewedBy" uuid,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_AITaskAssignmentSuggestions_MatchScore" CHECK ((("MatchScore" IS NULL) OR (("MatchScore" >= (0)::numeric) AND ("MatchScore" <= (1)::numeric))))
);


ALTER TABLE public."AITaskAssignmentSuggestions" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 252 (class 1259 OID 25442)
-- Name: Alerts; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Alerts" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid,
    "SensorId" uuid,
    "BatchId" uuid,
    "Severity" public."AlertSeverity" DEFAULT 'Medium'::public."AlertSeverity" NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Message" text,
    "IsResolved" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "ResolvedAt" timestamp without time zone
);


ALTER TABLE public."Alerts" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 225 (class 1259 OID 25033)
-- Name: Areas; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Areas" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "FarmId" uuid NOT NULL,
    "AreaCode" character varying(50) NOT NULL,
    "AreaName" character varying(100) NOT NULL,
    "EnvironmentType" character varying(50),
    "TotalArea" numeric(10,2),
    "Status" public."LocationStatus" DEFAULT 'Available'::public."LocationStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone,
    CONSTRAINT "CK_Areas_TotalArea_Positive" CHECK ((("TotalArea" IS NULL) OR ("TotalArea" > (0)::numeric)))
);


ALTER TABLE public."Areas" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 239 (class 1259 OID 25252)
-- Name: Batches; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Batches" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "ExperimentBedAssignmentId" uuid,
    "GroupId" uuid,
    "CropVarietyId" uuid,
    "BatchCode" character varying(50) NOT NULL,
    "PlantingDate" date,
    "ExpectedHarvestDate" date,
    "PlantCount" integer,
    "Status" public."BatchStatus" DEFAULT 'Planned'::public."BatchStatus" NOT NULL,
    "Notes" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone,
    CONSTRAINT "CK_Batches_DateRange" CHECK ((("PlantingDate" IS NULL) OR ("ExpectedHarvestDate" IS NULL) OR ("PlantingDate" <= "ExpectedHarvestDate"))),
    CONSTRAINT "CK_Batches_PlantCount" CHECK ((("PlantCount" IS NULL) OR ("PlantCount" >= 0)))
);


ALTER TABLE public."Batches" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 226 (class 1259 OID 25050)
-- Name: Beds; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Beds" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "AreaId" uuid NOT NULL,
    "BedCode" character varying(50) NOT NULL,
    "SoilDescription" text,
    "Length" numeric(10,2),
    "Width" numeric(10,2),
    "Status" public."LocationStatus" DEFAULT 'Available'::public."LocationStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone,
    CONSTRAINT "CK_Beds_Length_Positive" CHECK ((("Length" IS NULL) OR ("Length" > (0)::numeric))),
    CONSTRAINT "CK_Beds_Width_Positive" CHECK ((("Width" IS NULL) OR ("Width" > (0)::numeric)))
);


ALTER TABLE public."Beds" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 240 (class 1259 OID 25269)
-- Name: CareSchedules; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."CareSchedules" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "ExperimentStageId" uuid,
    "BatchId" uuid,
    "TaskType" public."TaskType" NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Instruction" text,
    "FrequencyDays" integer,
    "StartDate" date NOT NULL,
    "EndDate" date,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_CareSchedules_DateRange" CHECK ((("EndDate" IS NULL) OR ("StartDate" <= "EndDate"))),
    CONSTRAINT "CK_CareSchedules_Frequency" CHECK ((("FrequencyDays" IS NULL) OR ("FrequencyDays" > 0)))
);


ALTER TABLE public."CareSchedules" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 228 (class 1259 OID 25083)
-- Name: CropVarieties; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."CropVarieties" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "CropId" uuid NOT NULL,
    "VarietyName" character varying(100) NOT NULL,
    "Origin" character varying(100),
    "GrowthDurationDays" integer,
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone,
    CONSTRAINT "CK_CropVarieties_GrowthDuration" CHECK ((("GrowthDurationDays" IS NULL) OR ("GrowthDurationDays" > 0)))
);


ALTER TABLE public."CropVarieties" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 227 (class 1259 OID 25069)
-- Name: Crops; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Crops" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "CropName" character varying(100) NOT NULL,
    "ScientificName" character varying(150),
    "Category" character varying(100),
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone
);


ALTER TABLE public."Crops" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 238 (class 1259 OID 25239)
-- Name: ExperimentBedAssignments; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentBedAssignments" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "BedId" uuid NOT NULL,
    "AssignedFrom" date NOT NULL,
    "AssignedTo" date,
    "Purpose" text,
    CONSTRAINT "CK_ExperimentBedAssignments_DateRange" CHECK ((("AssignedTo" IS NULL) OR ("AssignedFrom" <= "AssignedTo")))
);


ALTER TABLE public."ExperimentBedAssignments" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 235 (class 1259 OID 25199)
-- Name: ExperimentDesigns; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentDesigns" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "DesignType" public."DesignType" DEFAULT 'CompletelyRandomized'::public."DesignType" NOT NULL,
    "ReplicationCount" integer,
    "RandomizationMethod" text,
    "DesignParameters" jsonb,
    CONSTRAINT "CK_ExperimentDesigns_Replication" CHECK ((("ReplicationCount" IS NULL) OR ("ReplicationCount" > 0)))
);


ALTER TABLE public."ExperimentDesigns" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 236 (class 1259 OID 25214)
-- Name: ExperimentGroups; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentGroups" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "GroupName" character varying(100) NOT NULL,
    "GroupType" public."GroupType" NOT NULL,
    "TreatmentDescription" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."ExperimentGroups" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 253 (class 1259 OID 25458)
-- Name: ExperimentReports; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentReports" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "CreatedBy" uuid,
    "ReportType" character varying(50) DEFAULT 'Summary'::character varying NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Summary" text,
    "ResultData" jsonb,
    "ExportFormat" character varying(20),
    "FileUrl" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."ExperimentReports" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 231 (class 1259 OID 25124)
-- Name: ExperimentRequests; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentRequests" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "FarmId" uuid NOT NULL,
    "ResearcherId" uuid NOT NULL,
    "CropVarietyId" uuid,
    "ProcedureTemplateId" uuid,
    "Title" character varying(200) NOT NULL,
    "Objective" text NOT NULL,
    "ExpectedStartDate" date,
    "ExpectedEndDate" date,
    "MonitoringPlan" jsonb,
    "Status" public."RequestStatus" DEFAULT 'Pending'::public."RequestStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_ExperimentRequests_DateRange" CHECK ((("ExpectedStartDate" IS NULL) OR ("ExpectedEndDate" IS NULL) OR ("ExpectedStartDate" <= "ExpectedEndDate")))
);


ALTER TABLE public."ExperimentRequests" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 234 (class 1259 OID 25181)
-- Name: ExperimentStages; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ExperimentStages" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "StageName" character varying(100) NOT NULL,
    "StageType" public."ExperimentStageType" NOT NULL,
    "StageOrder" integer NOT NULL,
    "Objective" text,
    "StartDate" date,
    "EndDate" date,
    "ResultSummary" text,
    "ResultData" jsonb,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_ExperimentStages_DateRange" CHECK ((("StartDate" IS NULL) OR ("EndDate" IS NULL) OR ("StartDate" <= "EndDate")))
);


ALTER TABLE public."ExperimentStages" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 233 (class 1259 OID 25158)
-- Name: Experiments; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Experiments" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "RequestId" uuid,
    "FarmId" uuid NOT NULL,
    "ResearcherId" uuid NOT NULL,
    "CropVarietyId" uuid,
    "ProcedureTemplateId" uuid,
    "ExperimentCode" character varying(50) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Objective" text NOT NULL,
    "Hypothesis" text,
    "StartDate" date,
    "EndDate" date,
    "Status" public."ExperimentStatus" DEFAULT 'Draft'::public."ExperimentStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone,
    CONSTRAINT "CK_Experiments_DateRange" CHECK ((("StartDate" IS NULL) OR ("EndDate" IS NULL) OR ("StartDate" <= "EndDate")))
);


ALTER TABLE public."Experiments" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 224 (class 1259 OID 25014)
-- Name: Farms; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Farms" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ManagerId" uuid,
    "FarmCode" character varying(50) NOT NULL,
    "FarmName" character varying(100) NOT NULL,
    "Location" character varying(255),
    "Description" text,
    "Status" public."LocationStatus" DEFAULT 'Available'::public."LocationStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone
);


ALTER TABLE public."Farms" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 255 (class 1259 OID 25487)
-- Name: KnowledgeDocumentChunks; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."KnowledgeDocumentChunks" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "DocumentId" uuid NOT NULL,
    "ChunkIndex" integer NOT NULL,
    "Content" text NOT NULL,
    "Embedding" text,
    "Metadata" jsonb
);


ALTER TABLE public."KnowledgeDocumentChunks" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 254 (class 1259 OID 25473)
-- Name: KnowledgeDocuments; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."KnowledgeDocuments" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "CropVarietyId" uuid,
    "Title" character varying(200) NOT NULL,
    "SourceUrl" text,
    "FileUrl" text,
    "DocumentStatus" public."DocumentStatus" DEFAULT 'Draft'::public."DocumentStatus" NOT NULL,
    "UploadedBy" uuid,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."KnowledgeDocuments" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 237 (class 1259 OID 25228)
-- Name: MeasurementDefinitions; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."MeasurementDefinitions" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "GroupId" uuid,
    "MetricName" character varying(100) NOT NULL,
    "Unit" character varying(30),
    "TargetValue" numeric(12,4),
    "Description" text
);


ALTER TABLE public."MeasurementDefinitions" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 245 (class 1259 OID 25342)
-- Name: MeasurementRecords; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."MeasurementRecords" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "ExperimentStageId" uuid,
    "BatchId" uuid NOT NULL,
    "MeasurementDefinitionId" uuid,
    "MeasuredBy" uuid,
    "Value" numeric(12,4),
    "TextValue" text,
    "ExtraData" jsonb,
    "MeasuredAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."MeasurementRecords" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 257 (class 1259 OID 25955)
-- Name: Notifications; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Notifications" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "RecipientId" uuid NOT NULL,
    "SenderId" uuid,
    "NotificationType" character varying(50) NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Message" text,
    "Priority" public."AlertSeverity" DEFAULT 'Low'::public."AlertSeverity" NOT NULL,
    "ReferenceTable" character varying(100),
    "ReferenceId" uuid,
    "Metadata" jsonb,
    "IsRead" boolean DEFAULT false NOT NULL,
    "ReadAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Notifications" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 247 (class 1259 OID 25368)
-- Name: PlantHealthAssessments; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."PlantHealthAssessments" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "BatchId" uuid,
    "ImageId" uuid,
    "AssessedBy" uuid,
    "AIModelName" character varying(100),
    "AIConfidence" numeric(5,4),
    "AISuggestion" text,
    "HumanConclusion" text,
    "ReviewStatus" public."AIReviewStatus" DEFAULT 'Suggested'::public."AIReviewStatus" NOT NULL,
    "AssessmentData" jsonb,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_PlantHealthAssessments_Confidence" CHECK ((("AIConfidence" IS NULL) OR (("AIConfidence" >= (0)::numeric) AND ("AIConfidence" <= (1)::numeric))))
);


ALTER TABLE public."PlantHealthAssessments" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 246 (class 1259 OID 25355)
-- Name: PlantImages; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."PlantImages" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "BatchId" uuid,
    "TaskReportId" uuid,
    "ImageUrl" text NOT NULL,
    "Caption" text,
    "UploadedBy" uuid,
    "CapturedAt" timestamp without time zone,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."PlantImages" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 230 (class 1259 OID 25109)
-- Name: ProcedureTemplateSteps; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ProcedureTemplateSteps" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "TemplateId" uuid NOT NULL,
    "StepOrder" integer NOT NULL,
    "StageType" public."ExperimentStageType" NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Instruction" text NOT NULL,
    "ExpectedDurationDays" integer,
    "RequiredSkillDescription" text,
    CONSTRAINT "CK_ProcedureTemplateSteps_Duration" CHECK ((("ExpectedDurationDays" IS NULL) OR ("ExpectedDurationDays" > 0)))
);


ALTER TABLE public."ProcedureTemplateSteps" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 229 (class 1259 OID 25097)
-- Name: ProcedureTemplates; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."ProcedureTemplates" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "CropVarietyId" uuid,
    "TemplateName" character varying(150) NOT NULL,
    "Objective" text,
    "Description" text,
    "CreatedBy" uuid,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."ProcedureTemplates" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 232 (class 1259 OID 25144)
-- Name: RequestReviews; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."RequestReviews" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "RequestId" uuid NOT NULL,
    "ReviewerId" uuid NOT NULL,
    "Result" public."ReviewResult" NOT NULL,
    "Comment" text,
    "ReviewedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."RequestReviews" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 219 (class 1259 OID 24943)
-- Name: Roles; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Roles" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "RoleName" character varying(50) NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Roles" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 250 (class 1259 OID 25414)
-- Name: SensorData; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."SensorData" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "SensorId" uuid NOT NULL,
    "ExperimentId" uuid,
    "BatchId" uuid,
    "Value" numeric(12,4) NOT NULL,
    "Unit" character varying(30),
    "RecordedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."SensorData" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 251 (class 1259 OID 25425)
-- Name: SensorThresholdRules; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."SensorThresholdRules" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "BatchId" uuid,
    "SensorType" public."SensorType" NOT NULL,
    "MinValue" numeric(12,4),
    "MaxValue" numeric(12,4),
    "Severity" public."AlertSeverity" DEFAULT 'Medium'::public."AlertSeverity" NOT NULL,
    "Message" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    CONSTRAINT "CK_SensorThresholdRules_MinMax" CHECK ((("MinValue" IS NULL) OR ("MaxValue" IS NULL) OR ("MinValue" <= "MaxValue"))),
    CONSTRAINT "CK_SensorThresholdRules_Range" CHECK ((("MinValue" IS NOT NULL) OR ("MaxValue" IS NOT NULL)))
);


ALTER TABLE public."SensorThresholdRules" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 249 (class 1259 OID 25399)
-- Name: Sensors; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Sensors" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "SensorCode" character varying(50) NOT NULL,
    "SensorType" public."SensorType" NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Sensors" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 222 (class 1259 OID 24986)
-- Name: Skills; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Skills" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "SkillName" character varying(100) NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Skills" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 256 (class 1259 OID 25934)
-- Name: SystemLogs; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."SystemLogs" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "UserId" uuid,
    "Action" character varying(100) NOT NULL,
    "EntityName" character varying(100),
    "EntityId" uuid,
    "Description" text,
    "IpAddress" character varying(45),
    "UserAgent" text,
    "Metadata" jsonb,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."SystemLogs" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 243 (class 1259 OID 25314)
-- Name: TaskAssignments; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."TaskAssignments" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "TaskId" uuid NOT NULL,
    "AssigneeId" uuid NOT NULL,
    "AssignedBy" uuid,
    "Status" public."TaskAssignmentStatus" DEFAULT 'Assigned'::public."TaskAssignmentStatus" NOT NULL,
    "Reason" text,
    "AssignedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "EndedAt" timestamp without time zone
);


ALTER TABLE public."TaskAssignments" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 244 (class 1259 OID 25329)
-- Name: TaskReports; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."TaskReports" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "TaskId" uuid NOT NULL,
    "ReporterId" uuid NOT NULL,
    "ReportText" text,
    "ResultData" jsonb,
    "ReportedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."TaskReports" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 242 (class 1259 OID 25304)
-- Name: TaskSkillRequirements; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."TaskSkillRequirements" (
    "TaskId" uuid NOT NULL,
    "SkillId" uuid NOT NULL,
    "RequiredLevel" integer DEFAULT 1 NOT NULL,
    CONSTRAINT "CK_TaskSkillRequirements_Level" CHECK ((("RequiredLevel" >= 1) AND ("RequiredLevel" <= 5)))
);


ALTER TABLE public."TaskSkillRequirements" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 241 (class 1259 OID 25286)
-- Name: Tasks; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Tasks" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ExperimentId" uuid NOT NULL,
    "ExperimentStageId" uuid,
    "BatchId" uuid,
    "CareScheduleId" uuid,
    "CreatedBy" uuid,
    "AssignedTo" uuid,
    "TaskType" public."TaskType" NOT NULL,
    "Title" character varying(150) NOT NULL,
    "Description" text,
    "RequiredSkillDescription" text,
    "DueDate" timestamp without time zone,
    "Status" public."TaskStatus" DEFAULT 'Pending'::public."TaskStatus" NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Tasks" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 221 (class 1259 OID 24977)
-- Name: UserRoles; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."UserRoles" (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "AssignedAt" timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."UserRoles" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 223 (class 1259 OID 25000)
-- Name: UserSkills; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."UserSkills" (
    "UserId" uuid NOT NULL,
    "SkillId" uuid NOT NULL,
    "ProficiencyLevel" integer DEFAULT 1 NOT NULL,
    "Description" text,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT "CK_UserSkills_Proficiency" CHECK ((("ProficiencyLevel" >= 1) AND ("ProficiencyLevel" <= 5)))
);


ALTER TABLE public."UserSkills" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 220 (class 1259 OID 24957)
-- Name: Users; Type: TABLE; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE TABLE public."Users" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "FullName" character varying(100) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "Phone" character varying(20),
    "ProfileDescription" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "DeletedAt" timestamp without time zone
);


ALTER TABLE public."Users" OWNER TO "SEP490_Smartfarm_DB";

--
-- TOC entry 4672 (class 0 OID 25383)
-- Dependencies: 248
-- Data for Name: AITaskAssignmentSuggestions; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."AITaskAssignmentSuggestions" ("Id", "TaskId", "SuggestedUserId", "MatchScore", "Reason", "ReviewStatus", "ReviewedBy", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4676 (class 0 OID 25442)
-- Dependencies: 252
-- Data for Name: Alerts; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Alerts" ("Id", "ExperimentId", "SensorId", "BatchId", "Severity", "Title", "Message", "IsResolved", "CreatedAt", "ResolvedAt") FROM stdin;
\.


--
-- TOC entry 4649 (class 0 OID 25033)
-- Dependencies: 225
-- Data for Name: Areas; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Areas" ("Id", "FarmId", "AreaCode", "AreaName", "EnvironmentType", "TotalArea", "Status", "CreatedAt", "UpdatedAt", "DeletedAt") FROM stdin;
c9965065-5104-405d-9fe1-9673bd7b86b4	6b9854c7-db55-4016-b250-d23f89afa225	a002	adasdasdads	asdasdadsdasd	3000000.00	Available	2026-06-17 09:50:53.410825	2026-06-17 09:50:53.410969	\N
b2dcf6db-29a4-4527-93bd-2d76a8c1e84d	c80eae9e-95d2-46e6-b96c-41272f18bf13	string333333	string	string	10000.00	Available	2026-06-18 07:41:01.727959	2026-06-18 07:44:40.445373	\N
\.


--
-- TOC entry 4663 (class 0 OID 25252)
-- Dependencies: 239
-- Data for Name: Batches; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Batches" ("Id", "ExperimentId", "ExperimentBedAssignmentId", "GroupId", "CropVarietyId", "BatchCode", "PlantingDate", "ExpectedHarvestDate", "PlantCount", "Status", "Notes", "CreatedAt", "DeletedAt") FROM stdin;
\.


--
-- TOC entry 4650 (class 0 OID 25050)
-- Dependencies: 226
-- Data for Name: Beds; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Beds" ("Id", "AreaId", "BedCode", "SoilDescription", "Length", "Width", "Status", "CreatedAt", "UpdatedAt", "DeletedAt") FROM stdin;
dcde9702-4de1-4ca2-95c4-9ec4a6f7ed83	b2dcf6db-29a4-4527-93bd-2d76a8c1e84d	string121212	string	1000.00	2000.00	Available	2026-06-18 07:43:41.686806	2026-06-18 07:44:21.410953	\N
\.


--
-- TOC entry 4664 (class 0 OID 25269)
-- Dependencies: 240
-- Data for Name: CareSchedules; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."CareSchedules" ("Id", "ExperimentId", "ExperimentStageId", "BatchId", "TaskType", "Title", "Instruction", "FrequencyDays", "StartDate", "EndDate", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4652 (class 0 OID 25083)
-- Dependencies: 228
-- Data for Name: CropVarieties; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."CropVarieties" ("Id", "CropId", "VarietyName", "Origin", "GrowthDurationDays", "Description", "CreatedAt", "DeletedAt") FROM stdin;
b38c30fb-b818-4aca-8cc6-0f7503fc2eb6	d82e3221-0ed9-445d-97a1-16fb5af50d5a	ca chua	string	14	trong dau cung duoc	2026-06-18 07:56:48.130147	\N
\.


--
-- TOC entry 4651 (class 0 OID 25069)
-- Dependencies: 227
-- Data for Name: Crops; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Crops" ("Id", "CropName", "ScientificName", "Category", "Description", "CreatedAt", "DeletedAt") FROM stdin;
d82e3221-0ed9-445d-97a1-16fb5af50d5a	ca chua	ca chua	an	chua	2026-06-18 07:54:09.697183	\N
\.


--
-- TOC entry 4662 (class 0 OID 25239)
-- Dependencies: 238
-- Data for Name: ExperimentBedAssignments; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentBedAssignments" ("Id", "ExperimentId", "BedId", "AssignedFrom", "AssignedTo", "Purpose") FROM stdin;
\.


--
-- TOC entry 4659 (class 0 OID 25199)
-- Dependencies: 235
-- Data for Name: ExperimentDesigns; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentDesigns" ("Id", "ExperimentId", "DesignType", "ReplicationCount", "RandomizationMethod", "DesignParameters") FROM stdin;
\.


--
-- TOC entry 4660 (class 0 OID 25214)
-- Dependencies: 236
-- Data for Name: ExperimentGroups; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentGroups" ("Id", "ExperimentId", "GroupName", "GroupType", "TreatmentDescription", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4677 (class 0 OID 25458)
-- Dependencies: 253
-- Data for Name: ExperimentReports; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentReports" ("Id", "ExperimentId", "CreatedBy", "ReportType", "Title", "Summary", "ResultData", "ExportFormat", "FileUrl", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4655 (class 0 OID 25124)
-- Dependencies: 231
-- Data for Name: ExperimentRequests; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentRequests" ("Id", "FarmId", "ResearcherId", "CropVarietyId", "ProcedureTemplateId", "Title", "Objective", "ExpectedStartDate", "ExpectedEndDate", "MonitoringPlan", "Status", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 4658 (class 0 OID 25181)
-- Dependencies: 234
-- Data for Name: ExperimentStages; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ExperimentStages" ("Id", "ExperimentId", "StageName", "StageType", "StageOrder", "Objective", "StartDate", "EndDate", "ResultSummary", "ResultData", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 4657 (class 0 OID 25158)
-- Dependencies: 233
-- Data for Name: Experiments; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Experiments" ("Id", "RequestId", "FarmId", "ResearcherId", "CropVarietyId", "ProcedureTemplateId", "ExperimentCode", "Title", "Objective", "Hypothesis", "StartDate", "EndDate", "Status", "CreatedAt", "UpdatedAt", "DeletedAt") FROM stdin;
\.


--
-- TOC entry 4648 (class 0 OID 25014)
-- Dependencies: 224
-- Data for Name: Farms; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Farms" ("Id", "ManagerId", "FarmCode", "FarmName", "Location", "Description", "Status", "CreatedAt", "UpdatedAt", "DeletedAt") FROM stdin;
6b9854c7-db55-4016-b250-d23f89afa225	f3dd3ab2-81fb-4000-b330-a2810532a10c	A001	cail ma	12312313123	1231231231231	Available	2026-06-17 09:50:10.742404	2026-06-17 09:50:10.742461	\N
c80eae9e-95d2-46e6-b96c-41272f18bf13	f3dd3ab2-81fb-4000-b330-a2810532a10c	string111111111	string	string	string	Available	2026-06-18 07:39:30.409571	2026-06-18 07:40:26.774042	\N
\.


--
-- TOC entry 4679 (class 0 OID 25487)
-- Dependencies: 255
-- Data for Name: KnowledgeDocumentChunks; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."KnowledgeDocumentChunks" ("Id", "DocumentId", "ChunkIndex", "Content", "Embedding", "Metadata") FROM stdin;
\.


--
-- TOC entry 4678 (class 0 OID 25473)
-- Dependencies: 254
-- Data for Name: KnowledgeDocuments; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."KnowledgeDocuments" ("Id", "CropVarietyId", "Title", "SourceUrl", "FileUrl", "DocumentStatus", "UploadedBy", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4661 (class 0 OID 25228)
-- Dependencies: 237
-- Data for Name: MeasurementDefinitions; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."MeasurementDefinitions" ("Id", "ExperimentId", "GroupId", "MetricName", "Unit", "TargetValue", "Description") FROM stdin;
\.


--
-- TOC entry 4669 (class 0 OID 25342)
-- Dependencies: 245
-- Data for Name: MeasurementRecords; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."MeasurementRecords" ("Id", "ExperimentId", "ExperimentStageId", "BatchId", "MeasurementDefinitionId", "MeasuredBy", "Value", "TextValue", "ExtraData", "MeasuredAt") FROM stdin;
\.


--
-- TOC entry 4681 (class 0 OID 25955)
-- Dependencies: 257
-- Data for Name: Notifications; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Notifications" ("Id", "RecipientId", "SenderId", "NotificationType", "Title", "Message", "Priority", "ReferenceTable", "ReferenceId", "Metadata", "IsRead", "ReadAt", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4671 (class 0 OID 25368)
-- Dependencies: 247
-- Data for Name: PlantHealthAssessments; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."PlantHealthAssessments" ("Id", "ExperimentId", "BatchId", "ImageId", "AssessedBy", "AIModelName", "AIConfidence", "AISuggestion", "HumanConclusion", "ReviewStatus", "AssessmentData", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4670 (class 0 OID 25355)
-- Dependencies: 246
-- Data for Name: PlantImages; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."PlantImages" ("Id", "ExperimentId", "BatchId", "TaskReportId", "ImageUrl", "Caption", "UploadedBy", "CapturedAt", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4654 (class 0 OID 25109)
-- Dependencies: 230
-- Data for Name: ProcedureTemplateSteps; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ProcedureTemplateSteps" ("Id", "TemplateId", "StepOrder", "StageType", "Title", "Instruction", "ExpectedDurationDays", "RequiredSkillDescription") FROM stdin;
96fb1fd8-0257-40ab-a835-1a46ac847259	7d471cc5-2422-48e0-a380-39eb4ac8e301	1	Nursery	string	string	7	string
\.


--
-- TOC entry 4653 (class 0 OID 25097)
-- Dependencies: 229
-- Data for Name: ProcedureTemplates; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."ProcedureTemplates" ("Id", "CropVarietyId", "TemplateName", "Objective", "Description", "CreatedBy", "CreatedAt") FROM stdin;
7d471cc5-2422-48e0-a380-39eb4ac8e301	b38c30fb-b818-4aca-8cc6-0f7503fc2eb6	string	string	string	b819bc5f-898f-42ff-a266-373876977a55	2026-06-18 16:48:12.186354
\.


--
-- TOC entry 4656 (class 0 OID 25144)
-- Dependencies: 232
-- Data for Name: RequestReviews; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."RequestReviews" ("Id", "RequestId", "ReviewerId", "Result", "Comment", "ReviewedAt") FROM stdin;
\.


--
-- TOC entry 4643 (class 0 OID 24943)
-- Dependencies: 219
-- Data for Name: Roles; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Roles" ("Id", "RoleName", "Description", "CreatedAt") FROM stdin;
62285744-8585-4813-9b2d-7605b3a5aaeb	Admin	Quản trị viên hệ thống	2026-06-17 08:39:34.023882
47561eab-307c-4c9f-b46e-3fb878ccf303	Manager	Quản lý nông trại	2026-06-17 08:39:34.023882
0b007278-98f4-42e2-8a54-4185b829b6f4	Technician	Kỹ thuật viên	2026-06-17 08:39:34.023882
dc05f974-1aea-4ed7-ac9c-e76e1d1313a6	Researcher	Nghiên cứu viên	2026-06-17 08:39:34.023882
07b9b480-e6e9-4a02-8bd8-64b957279265	Student	Học viên	2026-06-17 08:39:34.023882
\.


--
-- TOC entry 4674 (class 0 OID 25414)
-- Dependencies: 250
-- Data for Name: SensorData; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."SensorData" ("Id", "SensorId", "ExperimentId", "BatchId", "Value", "Unit", "RecordedAt") FROM stdin;
\.


--
-- TOC entry 4675 (class 0 OID 25425)
-- Dependencies: 251
-- Data for Name: SensorThresholdRules; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."SensorThresholdRules" ("Id", "ExperimentId", "BatchId", "SensorType", "MinValue", "MaxValue", "Severity", "Message", "IsActive") FROM stdin;
\.


--
-- TOC entry 4673 (class 0 OID 25399)
-- Dependencies: 249
-- Data for Name: Sensors; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Sensors" ("Id", "SensorCode", "SensorType", "Description", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4646 (class 0 OID 24986)
-- Dependencies: 222
-- Data for Name: Skills; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Skills" ("Id", "SkillName", "Description", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4680 (class 0 OID 25934)
-- Dependencies: 256
-- Data for Name: SystemLogs; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."SystemLogs" ("Id", "UserId", "Action", "EntityName", "EntityId", "Description", "IpAddress", "UserAgent", "Metadata", "CreatedAt") FROM stdin;
31c444f1-f060-470b-913f-a3356e778a59	06218f03-fa36-49b3-85cc-3816963516b8	LOGIN	Users	06218f03-fa36-49b3-85cc-3816963516b8	Người dùng Nam Hải Nam (maihainam8@gmail.com) đăng nhập thành công.	\N	\N	\N	2026-06-17 10:26:24.003589+00
3a5cce2a-b34f-422c-a6f1-c7768610ba48	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập qua Google.	\N	\N	\N	2026-06-17 10:37:36.924294+00
cde87080-15de-4fc1-b3dc-35318d3fa9e1	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập qua Google.	\N	\N	\N	2026-06-17 10:44:49.286251+00
3072637f-5404-43eb-b4ca-7d5fd8dadbc6	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập thành công.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-17 10:48:57.252942+00
c624f2db-8d56-461b-bcae-fe34111219bb	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập thành công.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-17 11:58:19.136938+00
9506269c-6076-4fca-9f4d-45ff2809f40f	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập thành công.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-17 12:01:10.552489+00
a92028d7-feea-4aba-a988-a25c047013be	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập thành công.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-17 12:05:24.919237+00
3fea3c9e-5f4a-43de-ae0e-222c6f87d100	551c3f5d-1b76-48a1-b106-ee123cfac79e	LOGIN	Users	551c3f5d-1b76-48a1-b106-ee123cfac79e	Người dùng Admin SmartFarm (chuongnn12.work@gmail.com) đăng nhập thành công.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-17 12:07:46.541976+00
120ba06f-2bba-48ef-b4a9-c31b376d5859	f3dd3ab2-81fb-4000-b330-a2810532a10c	LOGIN	Users	f3dd3ab2-81fb-4000-b330-a2810532a10c	Nguoi dung Manager (managersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Manager", "method": "local_auth"}	2026-06-18 07:37:30.445724+00
8d9d7095-9518-47a3-9dd4-d4132f3f3d55	f3dd3ab2-81fb-4000-b330-a2810532a10c	LOGIN	Users	f3dd3ab2-81fb-4000-b330-a2810532a10c	Nguoi dung Manager (managersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Manager", "method": "local_auth"}	2026-06-18 09:10:35.276222+00
1de66d83-2bac-47c2-b57c-720fd80e169b	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 09:12:27.965228+00
2c93d6f0-db90-4795-9076-1f2646c7a024	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 09:42:30.905159+00
fe5d98df-292f-4987-9c83-009a77725035	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 14:30:31.604362+00
6b9bfd29-e8bf-4950-b880-6aceb63511e5	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:00:23.625472+00
abf39b5d-678d-4a6b-9456-6d77efab47d8	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:16:39.881838+00
41fe44ff-b7d1-4785-9672-1277afeab8af	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:21:49.817977+00
279d36b5-20df-4e72-b723-4e5420987c17	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:28:35.531827+00
3c2b80f4-5aac-423e-8002-474bec16789c	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:37:57.93356+00
bf2b5bb8-3f68-4b49-8168-40c2149a3983	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 15:43:07.788739+00
f1363c48-a852-4be6-8cdc-7315e42cc328	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:00:30.491419+00
e8cb18fb-a170-4f76-ab08-5c7da9db8f19	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:09:53.431461+00
7bbc5141-1dc7-4ea7-ad39-8da5c1194f7c	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:18:41.258055+00
65acb20d-5195-43c7-958a-9a8d03efe4dd	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:27:10.502827+00
e700a60e-36af-4cbb-822e-b6c788e8a117	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:41:25.398839+00
ce908fc8-d889-4523-a307-50109eab4c96	b819bc5f-898f-42ff-a266-373876977a55	LOGIN	Users	b819bc5f-898f-42ff-a266-373876977a55	Nguoi dung Researcher (researchersmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Researcher", "method": "local_auth"}	2026-06-18 16:47:23.356212+00
8122baef-a9a1-4b7e-bdad-5aed8eee0cfd	06218f03-fa36-49b3-85cc-3816963516b8	LOGIN	Users	06218f03-fa36-49b3-85cc-3816963516b8	Nguoi dung Mai Hải Nam (maihainam8@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Admin", "method": "local_auth"}	2026-06-19 14:47:14.778847+00
9c2fa60e-12b5-47c8-b6c9-a8819f3fc5f3	349a8f16-eaea-489a-b016-af358ac2da47	LOGIN	Users	349a8f16-eaea-489a-b016-af358ac2da47	Nguoi dung Student (studentsmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Student", "method": "local_auth"}	2026-06-19 14:51:42.413665+00
2fb76d75-2411-4c20-9798-08795fe731e0	349a8f16-eaea-489a-b016-af358ac2da47	LOGIN	Users	349a8f16-eaea-489a-b016-af358ac2da47	Nguoi dung Student (studentsmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Student", "method": "local_auth"}	2026-06-19 14:57:39.161872+00
707bf065-f5d9-4ebc-947e-2733207a6a56	349a8f16-eaea-489a-b016-af358ac2da47	LOGIN	Users	349a8f16-eaea-489a-b016-af358ac2da47	Nguoi dung Student (studentsmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Student", "method": "local_auth"}	2026-06-19 15:05:44.304429+00
520e4d90-7124-4c42-859b-5413a01ef4ba	349a8f16-eaea-489a-b016-af358ac2da47	LOGIN	Users	349a8f16-eaea-489a-b016-af358ac2da47	Nguoi dung Student (studentsmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Student", "method": "local_auth"}	2026-06-19 15:06:25.132133+00
504364b6-19c8-4a9a-9b66-8c92eed58ef6	349a8f16-eaea-489a-b016-af358ac2da47	LOGIN	Users	349a8f16-eaea-489a-b016-af358ac2da47	Nguoi dung Student (studentsmartfarm@gmail.com) dang nhap thanh cong.	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36	{"role": "Student", "method": "local_auth"}	2026-06-19 15:07:34.305973+00
\.


--
-- TOC entry 4667 (class 0 OID 25314)
-- Dependencies: 243
-- Data for Name: TaskAssignments; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."TaskAssignments" ("Id", "TaskId", "AssigneeId", "AssignedBy", "Status", "Reason", "AssignedAt", "EndedAt") FROM stdin;
\.


--
-- TOC entry 4668 (class 0 OID 25329)
-- Dependencies: 244
-- Data for Name: TaskReports; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."TaskReports" ("Id", "TaskId", "ReporterId", "ReportText", "ResultData", "ReportedAt") FROM stdin;
\.


--
-- TOC entry 4666 (class 0 OID 25304)
-- Dependencies: 242
-- Data for Name: TaskSkillRequirements; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."TaskSkillRequirements" ("TaskId", "SkillId", "RequiredLevel") FROM stdin;
\.


--
-- TOC entry 4665 (class 0 OID 25286)
-- Dependencies: 241
-- Data for Name: Tasks; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Tasks" ("Id", "ExperimentId", "ExperimentStageId", "BatchId", "CareScheduleId", "CreatedBy", "AssignedTo", "TaskType", "Title", "Description", "RequiredSkillDescription", "DueDate", "Status", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- TOC entry 4645 (class 0 OID 24977)
-- Dependencies: 221
-- Data for Name: UserRoles; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."UserRoles" ("UserId", "RoleId", "AssignedAt") FROM stdin;
551c3f5d-1b76-48a1-b106-ee123cfac79e	62285744-8585-4813-9b2d-7605b3a5aaeb	2026-06-17 08:41:52.34809
72413116-4f30-4e7c-917b-da8394d2fb31	62285744-8585-4813-9b2d-7605b3a5aaeb	2026-06-17 09:26:51.581403
06218f03-fa36-49b3-85cc-3816963516b8	62285744-8585-4813-9b2d-7605b3a5aaeb	2026-06-17 09:27:40.000707
61ce36e5-1f21-46a8-aeea-498760d86988	62285744-8585-4813-9b2d-7605b3a5aaeb	2026-06-17 09:28:19.397425
c1507adb-069c-4e60-acc3-ec5c3d7cbbb8	62285744-8585-4813-9b2d-7605b3a5aaeb	2026-06-17 09:29:38.906918
f3dd3ab2-81fb-4000-b330-a2810532a10c	47561eab-307c-4c9f-b46e-3fb878ccf303	2026-06-17 09:34:23.421935
9d2c4862-8e2e-46cf-9a42-5b85ca4677fc	0b007278-98f4-42e2-8a54-4185b829b6f4	2026-06-17 09:34:56.391783
b819bc5f-898f-42ff-a266-373876977a55	dc05f974-1aea-4ed7-ac9c-e76e1d1313a6	2026-06-17 09:35:28.295607
349a8f16-eaea-489a-b016-af358ac2da47	07b9b480-e6e9-4a02-8bd8-64b957279265	2026-06-17 09:35:54.319893
\.


--
-- TOC entry 4647 (class 0 OID 25000)
-- Dependencies: 223
-- Data for Name: UserSkills; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."UserSkills" ("UserId", "SkillId", "ProficiencyLevel", "Description", "CreatedAt") FROM stdin;
\.


--
-- TOC entry 4644 (class 0 OID 24957)
-- Dependencies: 220
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: SEP490_Smartfarm_DB
--

COPY public."Users" ("Id", "FullName", "Email", "PasswordHash", "Phone", "ProfileDescription", "IsActive", "CreatedAt", "UpdatedAt", "DeletedAt") FROM stdin;
551c3f5d-1b76-48a1-b106-ee123cfac79e	Admin SmartFarm	chuongnn12.work@gmail.com	$2a$11$pNFs7KdGDc9mOzdj/QvhM.X0BszLl1zJ0CxRz7F9b3p11O27xHIm6	\N	\N	t	2026-06-17 08:35:07.401769	2026-06-17 08:56:36.084648	\N
72413116-4f30-4e7c-917b-da8394d2fb31	Nguyễn Mai Thành Nam	namqt1010@gmail.com	$2a$11$x9O.uqMWBjdhPTvr5ChXjOevPPBwRjwUddVQnIl73mJUS/012LcUe	0182398123	adssdas	t	2026-06-17 09:26:51.581366	2026-06-17 09:26:51.58138	\N
61ce36e5-1f21-46a8-aeea-498760d86988	Trương Tuấn Vũ	truongtuanvu12012004@gmail.com	$2a$11$1mqZHaKZmqdn8nP.eB0iXOsgI0Epry49NWqAwkqlSX0TfVFxQ2DLi	019239812	áddasdas	t	2026-06-17 09:28:19.397423	2026-06-17 09:28:19.397423	\N
c1507adb-069c-4e60-acc3-ec5c3d7cbbb8	Nguyễn Quốc Đạt 	quocdat2004.hs@gmail.com	$2a$11$BgciuW6d0rdEZPaeNedw2uhXwSKGb3SDAeRFI5KazMgAKyIb3UKXS	091278312313	adasdas	t	2026-06-17 09:29:38.906916	2026-06-17 09:29:38.906917	\N
f3dd3ab2-81fb-4000-b330-a2810532a10c	Manager	managersmartfarm@gmail.com	$2a$11$V2/N3AhSVFek.GUudFQfH.HJQeaCUCGTdvLvhZMgx40HMVFXiJE7W	091328123123	123123	t	2026-06-17 09:34:23.421933	2026-06-17 09:34:23.421934	\N
9d2c4862-8e2e-46cf-9a42-5b85ca4677fc	Technician	techniciansmartfarm@gmail.com	$2a$11$6z/XBahqkkYy/KUI.jvXhup5PlyYPCWo5VRXeNFLHsfhVpPH.ssz2	0912378281	adasdasd	t	2026-06-17 09:34:56.39178	2026-06-17 09:34:56.391781	\N
b819bc5f-898f-42ff-a266-373876977a55	Researcher	researchersmartfarm@gmail.com	$2a$11$aZ8S1lxx/Qq4FBNU/QupaOJ62FRl7xysOD3bKHtqfEQEvApBYKBAq	09123912312	ádasd	t	2026-06-17 09:35:28.295605	2026-06-17 09:35:28.295606	\N
349a8f16-eaea-489a-b016-af358ac2da47	Student	studentsmartfarm@gmail.com	$2a$11$YISAT0sv2pxqKXTlLHC2Q.PQbfZyjubgiQkQuvroNpB4BUOroRaGO	0912378121	adasda	t	2026-06-17 09:35:54.31989	2026-06-17 09:35:54.319891	\N
06218f03-fa36-49b3-85cc-3816963516b8	Mai Hải Nam	maihainam8@gmail.com	$2a$11$/EkaDF4XmigKMZsSbmMIbOEm1apZXmnV5nnYUj.sjFQXVWops0RG6	0912381212	đâs	t	2026-06-17 09:14:17.81984	2026-06-17 11:07:11.139614	2026-06-17 10:44:14.389813
\.


--
-- TOC entry 4378 (class 2606 OID 25398)
-- Name: AITaskAssignmentSuggestions AITaskAssignmentSuggestions_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."AITaskAssignmentSuggestions"
    ADD CONSTRAINT "AITaskAssignmentSuggestions_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4392 (class 2606 OID 25457)
-- Name: Alerts Alerts_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT "Alerts_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4296 (class 2606 OID 25049)
-- Name: Areas Areas_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Areas"
    ADD CONSTRAINT "Areas_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4347 (class 2606 OID 25268)
-- Name: Batches Batches_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "Batches_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4300 (class 2606 OID 25068)
-- Name: Beds Beds_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Beds"
    ADD CONSTRAINT "Beds_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4352 (class 2606 OID 25285)
-- Name: CareSchedules CareSchedules_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CareSchedules"
    ADD CONSTRAINT "CareSchedules_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4308 (class 2606 OID 25096)
-- Name: CropVarieties CropVarieties_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CropVarieties"
    ADD CONSTRAINT "CropVarieties_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4304 (class 2606 OID 25082)
-- Name: Crops Crops_CropName_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Crops"
    ADD CONSTRAINT "Crops_CropName_key" UNIQUE ("CropName");


--
-- TOC entry 4306 (class 2606 OID 25080)
-- Name: Crops Crops_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Crops"
    ADD CONSTRAINT "Crops_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4344 (class 2606 OID 25251)
-- Name: ExperimentBedAssignments ExperimentBedAssignments_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentBedAssignments"
    ADD CONSTRAINT "ExperimentBedAssignments_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4332 (class 2606 OID 25213)
-- Name: ExperimentDesigns ExperimentDesigns_ExperimentId_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentDesigns"
    ADD CONSTRAINT "ExperimentDesigns_ExperimentId_key" UNIQUE ("ExperimentId");


--
-- TOC entry 4334 (class 2606 OID 25211)
-- Name: ExperimentDesigns ExperimentDesigns_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentDesigns"
    ADD CONSTRAINT "ExperimentDesigns_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4336 (class 2606 OID 25227)
-- Name: ExperimentGroups ExperimentGroups_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentGroups"
    ADD CONSTRAINT "ExperimentGroups_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4395 (class 2606 OID 25472)
-- Name: ExperimentReports ExperimentReports_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentReports"
    ADD CONSTRAINT "ExperimentReports_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4317 (class 2606 OID 25143)
-- Name: ExperimentRequests ExperimentRequests_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentRequests"
    ADD CONSTRAINT "ExperimentRequests_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4328 (class 2606 OID 25198)
-- Name: ExperimentStages ExperimentStages_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentStages"
    ADD CONSTRAINT "ExperimentStages_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4323 (class 2606 OID 25180)
-- Name: Experiments Experiments_ExperimentCode_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_ExperimentCode_key" UNIQUE ("ExperimentCode");


--
-- TOC entry 4325 (class 2606 OID 25178)
-- Name: Experiments Experiments_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4291 (class 2606 OID 25032)
-- Name: Farms Farms_FarmCode_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Farms"
    ADD CONSTRAINT "Farms_FarmCode_key" UNIQUE ("FarmCode");


--
-- TOC entry 4293 (class 2606 OID 25030)
-- Name: Farms Farms_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Farms"
    ADD CONSTRAINT "Farms_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4402 (class 2606 OID 25498)
-- Name: KnowledgeDocumentChunks KnowledgeDocumentChunks_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."KnowledgeDocumentChunks"
    ADD CONSTRAINT "KnowledgeDocumentChunks_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4399 (class 2606 OID 25486)
-- Name: KnowledgeDocuments KnowledgeDocuments_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."KnowledgeDocuments"
    ADD CONSTRAINT "KnowledgeDocuments_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4341 (class 2606 OID 25238)
-- Name: MeasurementDefinitions MeasurementDefinitions_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementDefinitions"
    ADD CONSTRAINT "MeasurementDefinitions_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4370 (class 2606 OID 25354)
-- Name: MeasurementRecords MeasurementRecords_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4414 (class 2606 OID 25972)
-- Name: Notifications Notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "Notifications_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4376 (class 2606 OID 25382)
-- Name: PlantHealthAssessments PlantHealthAssessments_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantHealthAssessments"
    ADD CONSTRAINT "PlantHealthAssessments_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4373 (class 2606 OID 25367)
-- Name: PlantImages PlantImages_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantImages"
    ADD CONSTRAINT "PlantImages_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4314 (class 2606 OID 25123)
-- Name: ProcedureTemplateSteps ProcedureTemplateSteps_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ProcedureTemplateSteps"
    ADD CONSTRAINT "ProcedureTemplateSteps_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4312 (class 2606 OID 25108)
-- Name: ProcedureTemplates ProcedureTemplates_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ProcedureTemplates"
    ADD CONSTRAINT "ProcedureTemplates_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4321 (class 2606 OID 25157)
-- Name: RequestReviews RequestReviews_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."RequestReviews"
    ADD CONSTRAINT "RequestReviews_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4272 (class 2606 OID 24956)
-- Name: Roles Roles_RoleName_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "Roles_RoleName_key" UNIQUE ("RoleName");


--
-- TOC entry 4274 (class 2606 OID 24954)
-- Name: Roles Roles_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "Roles_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4387 (class 2606 OID 25424)
-- Name: SensorData SensorData_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorData"
    ADD CONSTRAINT "SensorData_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4390 (class 2606 OID 25441)
-- Name: SensorThresholdRules SensorThresholdRules_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorThresholdRules"
    ADD CONSTRAINT "SensorThresholdRules_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4381 (class 2606 OID 25413)
-- Name: Sensors Sensors_SensorCode_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Sensors"
    ADD CONSTRAINT "Sensors_SensorCode_key" UNIQUE ("SensorCode");


--
-- TOC entry 4383 (class 2606 OID 25411)
-- Name: Sensors Sensors_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Sensors"
    ADD CONSTRAINT "Sensors_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4284 (class 2606 OID 24999)
-- Name: Skills Skills_SkillName_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Skills"
    ADD CONSTRAINT "Skills_SkillName_key" UNIQUE ("SkillName");


--
-- TOC entry 4286 (class 2606 OID 24997)
-- Name: Skills Skills_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Skills"
    ADD CONSTRAINT "Skills_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4409 (class 2606 OID 25945)
-- Name: SystemLogs SystemLogs_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SystemLogs"
    ADD CONSTRAINT "SystemLogs_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4363 (class 2606 OID 25328)
-- Name: TaskAssignments TaskAssignments_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskAssignments"
    ADD CONSTRAINT "TaskAssignments_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4366 (class 2606 OID 25341)
-- Name: TaskReports TaskReports_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskReports"
    ADD CONSTRAINT "TaskReports_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4359 (class 2606 OID 25313)
-- Name: TaskSkillRequirements TaskSkillRequirements_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskSkillRequirements"
    ADD CONSTRAINT "TaskSkillRequirements_pkey" PRIMARY KEY ("TaskId", "SkillId");


--
-- TOC entry 4357 (class 2606 OID 25303)
-- Name: Tasks Tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4282 (class 2606 OID 24985)
-- Name: UserRoles UserRoles_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "UserRoles_pkey" PRIMARY KEY ("UserId", "RoleId");


--
-- TOC entry 4289 (class 2606 OID 25013)
-- Name: UserSkills UserSkills_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserSkills"
    ADD CONSTRAINT "UserSkills_pkey" PRIMARY KEY ("UserId", "SkillId");


--
-- TOC entry 4277 (class 2606 OID 24976)
-- Name: Users Users_Email_key; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_Email_key" UNIQUE ("Email");


--
-- TOC entry 4279 (class 2606 OID 24974)
-- Name: Users Users_pkey; Type: CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4379 (class 1259 OID 25533)
-- Name: IX_AITaskAssignmentSuggestions_TaskId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_AITaskAssignmentSuggestions_TaskId" ON public."AITaskAssignmentSuggestions" USING btree ("TaskId");


--
-- TOC entry 4393 (class 1259 OID 25537)
-- Name: IX_Alerts_Experiment_Resolved; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Alerts_Experiment_Resolved" ON public."Alerts" USING btree ("ExperimentId", "IsResolved");


--
-- TOC entry 4297 (class 1259 OID 25504)
-- Name: IX_Areas_FarmId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Areas_FarmId" ON public."Areas" USING btree ("FarmId");


--
-- TOC entry 4348 (class 1259 OID 25521)
-- Name: IX_Batches_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Batches_ExperimentId" ON public."Batches" USING btree ("ExperimentId");


--
-- TOC entry 4349 (class 1259 OID 25522)
-- Name: IX_Batches_GroupId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Batches_GroupId" ON public."Batches" USING btree ("GroupId");


--
-- TOC entry 4301 (class 1259 OID 25506)
-- Name: IX_Beds_AreaId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Beds_AreaId" ON public."Beds" USING btree ("AreaId");


--
-- TOC entry 4353 (class 1259 OID 25523)
-- Name: IX_CareSchedules_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_CareSchedules_ExperimentId" ON public."CareSchedules" USING btree ("ExperimentId");


--
-- TOC entry 4309 (class 1259 OID 25508)
-- Name: IX_CropVarieties_CropId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_CropVarieties_CropId" ON public."CropVarieties" USING btree ("CropId");


--
-- TOC entry 4345 (class 1259 OID 25519)
-- Name: IX_ExperimentBedAssignments_BedId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_ExperimentBedAssignments_BedId" ON public."ExperimentBedAssignments" USING btree ("BedId");


--
-- TOC entry 4337 (class 1259 OID 25516)
-- Name: IX_ExperimentGroups_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_ExperimentGroups_ExperimentId" ON public."ExperimentGroups" USING btree ("ExperimentId");


--
-- TOC entry 4396 (class 1259 OID 25538)
-- Name: IX_ExperimentReports_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_ExperimentReports_ExperimentId" ON public."ExperimentReports" USING btree ("ExperimentId");


--
-- TOC entry 4318 (class 1259 OID 25510)
-- Name: IX_ExperimentRequests_Researcher_Status; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_ExperimentRequests_Researcher_Status" ON public."ExperimentRequests" USING btree ("ResearcherId", "Status");


--
-- TOC entry 4329 (class 1259 OID 25514)
-- Name: IX_ExperimentStages_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_ExperimentStages_ExperimentId" ON public."ExperimentStages" USING btree ("ExperimentId");


--
-- TOC entry 4326 (class 1259 OID 25512)
-- Name: IX_Experiments_Researcher_Status; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Experiments_Researcher_Status" ON public."Experiments" USING btree ("ResearcherId", "Status");


--
-- TOC entry 4294 (class 1259 OID 25502)
-- Name: IX_Farms_ManagerId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Farms_ManagerId" ON public."Farms" USING btree ("ManagerId");


--
-- TOC entry 4400 (class 1259 OID 25541)
-- Name: IX_KnowledgeDocumentChunks_DocumentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_KnowledgeDocumentChunks_DocumentId" ON public."KnowledgeDocumentChunks" USING btree ("DocumentId");


--
-- TOC entry 4397 (class 1259 OID 25539)
-- Name: IX_KnowledgeDocuments_CropVarietyId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_KnowledgeDocuments_CropVarietyId" ON public."KnowledgeDocuments" USING btree ("CropVarietyId");


--
-- TOC entry 4339 (class 1259 OID 25518)
-- Name: IX_MeasurementDefinitions_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_MeasurementDefinitions_ExperimentId" ON public."MeasurementDefinitions" USING btree ("ExperimentId");


--
-- TOC entry 4367 (class 1259 OID 25529)
-- Name: IX_MeasurementRecords_Batch_MeasuredAt; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_MeasurementRecords_Batch_MeasuredAt" ON public."MeasurementRecords" USING btree ("BatchId", "MeasuredAt");


--
-- TOC entry 4368 (class 1259 OID 25530)
-- Name: IX_MeasurementRecords_DefinitionId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_MeasurementRecords_DefinitionId" ON public."MeasurementRecords" USING btree ("MeasurementDefinitionId");


--
-- TOC entry 4410 (class 1259 OID 25984)
-- Name: IX_Notifications_Recipient_CreatedAt; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Notifications_Recipient_CreatedAt" ON public."Notifications" USING btree ("RecipientId", "CreatedAt");


--
-- TOC entry 4411 (class 1259 OID 25983)
-- Name: IX_Notifications_Recipient_Read; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Notifications_Recipient_Read" ON public."Notifications" USING btree ("RecipientId", "IsRead");


--
-- TOC entry 4412 (class 1259 OID 25985)
-- Name: IX_Notifications_Reference; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Notifications_Reference" ON public."Notifications" USING btree ("ReferenceTable", "ReferenceId");


--
-- TOC entry 4374 (class 1259 OID 25532)
-- Name: IX_PlantHealthAssessments_BatchId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_PlantHealthAssessments_BatchId" ON public."PlantHealthAssessments" USING btree ("BatchId");


--
-- TOC entry 4371 (class 1259 OID 25531)
-- Name: IX_PlantImages_BatchId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_PlantImages_BatchId" ON public."PlantImages" USING btree ("BatchId");


--
-- TOC entry 4319 (class 1259 OID 25511)
-- Name: IX_RequestReviews_RequestId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_RequestReviews_RequestId" ON public."RequestReviews" USING btree ("RequestId");


--
-- TOC entry 4384 (class 1259 OID 25535)
-- Name: IX_SensorData_Experiment_Batch; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SensorData_Experiment_Batch" ON public."SensorData" USING btree ("ExperimentId", "BatchId");


--
-- TOC entry 4385 (class 1259 OID 25534)
-- Name: IX_SensorData_Sensor_RecordedAt; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SensorData_Sensor_RecordedAt" ON public."SensorData" USING btree ("SensorId", "RecordedAt");


--
-- TOC entry 4388 (class 1259 OID 25536)
-- Name: IX_SensorThresholdRules_ExperimentId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SensorThresholdRules_ExperimentId" ON public."SensorThresholdRules" USING btree ("ExperimentId");


--
-- TOC entry 4404 (class 1259 OID 25952)
-- Name: IX_SystemLogs_Action; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SystemLogs_Action" ON public."SystemLogs" USING btree ("Action");


--
-- TOC entry 4405 (class 1259 OID 25954)
-- Name: IX_SystemLogs_CreatedAt; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SystemLogs_CreatedAt" ON public."SystemLogs" USING btree ("CreatedAt");


--
-- TOC entry 4406 (class 1259 OID 25953)
-- Name: IX_SystemLogs_Entity; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SystemLogs_Entity" ON public."SystemLogs" USING btree ("EntityName", "EntityId");


--
-- TOC entry 4407 (class 1259 OID 25951)
-- Name: IX_SystemLogs_UserId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_SystemLogs_UserId" ON public."SystemLogs" USING btree ("UserId");


--
-- TOC entry 4360 (class 1259 OID 25527)
-- Name: IX_TaskAssignments_Assignee_Status; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_TaskAssignments_Assignee_Status" ON public."TaskAssignments" USING btree ("AssigneeId", "Status");


--
-- TOC entry 4361 (class 1259 OID 25526)
-- Name: IX_TaskAssignments_TaskId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_TaskAssignments_TaskId" ON public."TaskAssignments" USING btree ("TaskId");


--
-- TOC entry 4364 (class 1259 OID 25528)
-- Name: IX_TaskReports_TaskId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_TaskReports_TaskId" ON public."TaskReports" USING btree ("TaskId");


--
-- TOC entry 4354 (class 1259 OID 25525)
-- Name: IX_Tasks_AssignedTo_Status; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Tasks_AssignedTo_Status" ON public."Tasks" USING btree ("AssignedTo", "Status");


--
-- TOC entry 4355 (class 1259 OID 25524)
-- Name: IX_Tasks_Experiment_Status; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Tasks_Experiment_Status" ON public."Tasks" USING btree ("ExperimentId", "Status");


--
-- TOC entry 4280 (class 1259 OID 25500)
-- Name: IX_UserRoles_RoleId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_UserRoles_RoleId" ON public."UserRoles" USING btree ("RoleId");


--
-- TOC entry 4287 (class 1259 OID 25501)
-- Name: IX_UserSkills_SkillId; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_UserSkills_SkillId" ON public."UserSkills" USING btree ("SkillId");


--
-- TOC entry 4275 (class 1259 OID 25499)
-- Name: IX_Users_Email_Active; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE INDEX "IX_Users_Email_Active" ON public."Users" USING btree ("Email");


--
-- TOC entry 4298 (class 1259 OID 25503)
-- Name: UQ_Areas_Farm_Code; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_Areas_Farm_Code" ON public."Areas" USING btree ("FarmId", "AreaCode");


--
-- TOC entry 4350 (class 1259 OID 25520)
-- Name: UQ_Batches_Experiment_Code; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_Batches_Experiment_Code" ON public."Batches" USING btree ("ExperimentId", "BatchCode");


--
-- TOC entry 4302 (class 1259 OID 25505)
-- Name: UQ_Beds_Area_Code; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_Beds_Area_Code" ON public."Beds" USING btree ("AreaId", "BedCode");


--
-- TOC entry 4310 (class 1259 OID 25507)
-- Name: UQ_CropVarieties_Crop_Name; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_CropVarieties_Crop_Name" ON public."CropVarieties" USING btree ("CropId", "VarietyName");


--
-- TOC entry 4338 (class 1259 OID 25515)
-- Name: UQ_ExperimentGroups_Name; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_ExperimentGroups_Name" ON public."ExperimentGroups" USING btree ("ExperimentId", "GroupName");


--
-- TOC entry 4330 (class 1259 OID 25513)
-- Name: UQ_ExperimentStages_Order; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_ExperimentStages_Order" ON public."ExperimentStages" USING btree ("ExperimentId", "StageOrder");


--
-- TOC entry 4403 (class 1259 OID 25540)
-- Name: UQ_KnowledgeDocumentChunks_Index; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_KnowledgeDocumentChunks_Index" ON public."KnowledgeDocumentChunks" USING btree ("DocumentId", "ChunkIndex");


--
-- TOC entry 4342 (class 1259 OID 25517)
-- Name: UQ_MeasurementDefinitions_Scope; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_MeasurementDefinitions_Scope" ON public."MeasurementDefinitions" USING btree ("ExperimentId", "GroupId", "MetricName");


--
-- TOC entry 4315 (class 1259 OID 25509)
-- Name: UQ_ProcedureTemplateSteps_Order; Type: INDEX; Schema: public; Owner: SEP490_Smartfarm_DB
--

CREATE UNIQUE INDEX "UQ_ProcedureTemplateSteps_Order" ON public."ProcedureTemplateSteps" USING btree ("TemplateId", "StepOrder");


--
-- TOC entry 4477 (class 2606 OID 25862)
-- Name: AITaskAssignmentSuggestions AITaskAssignmentSuggestions_ReviewedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."AITaskAssignmentSuggestions"
    ADD CONSTRAINT "AITaskAssignmentSuggestions_ReviewedBy_fkey" FOREIGN KEY ("ReviewedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4478 (class 2606 OID 25857)
-- Name: AITaskAssignmentSuggestions AITaskAssignmentSuggestions_SuggestedUserId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."AITaskAssignmentSuggestions"
    ADD CONSTRAINT "AITaskAssignmentSuggestions_SuggestedUserId_fkey" FOREIGN KEY ("SuggestedUserId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4479 (class 2606 OID 25852)
-- Name: AITaskAssignmentSuggestions AITaskAssignmentSuggestions_TaskId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."AITaskAssignmentSuggestions"
    ADD CONSTRAINT "AITaskAssignmentSuggestions_TaskId_fkey" FOREIGN KEY ("TaskId") REFERENCES public."Tasks"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4485 (class 2606 OID 25902)
-- Name: Alerts Alerts_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT "Alerts_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4486 (class 2606 OID 25892)
-- Name: Alerts Alerts_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT "Alerts_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4487 (class 2606 OID 25897)
-- Name: Alerts Alerts_SensorId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT "Alerts_SensorId_fkey" FOREIGN KEY ("SensorId") REFERENCES public."Sensors"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4420 (class 2606 OID 25567)
-- Name: Areas Areas_FarmId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Areas"
    ADD CONSTRAINT "Areas_FarmId_fkey" FOREIGN KEY ("FarmId") REFERENCES public."Farms"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4444 (class 2606 OID 25702)
-- Name: Batches Batches_CropVarietyId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "Batches_CropVarietyId_fkey" FOREIGN KEY ("CropVarietyId") REFERENCES public."CropVarieties"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4445 (class 2606 OID 25692)
-- Name: Batches Batches_ExperimentBedAssignmentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "Batches_ExperimentBedAssignmentId_fkey" FOREIGN KEY ("ExperimentBedAssignmentId") REFERENCES public."ExperimentBedAssignments"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4446 (class 2606 OID 25687)
-- Name: Batches Batches_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "Batches_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4447 (class 2606 OID 25697)
-- Name: Batches Batches_GroupId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "Batches_GroupId_fkey" FOREIGN KEY ("GroupId") REFERENCES public."ExperimentGroups"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4421 (class 2606 OID 25572)
-- Name: Beds Beds_AreaId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Beds"
    ADD CONSTRAINT "Beds_AreaId_fkey" FOREIGN KEY ("AreaId") REFERENCES public."Areas"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4448 (class 2606 OID 25717)
-- Name: CareSchedules CareSchedules_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CareSchedules"
    ADD CONSTRAINT "CareSchedules_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4449 (class 2606 OID 25707)
-- Name: CareSchedules CareSchedules_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CareSchedules"
    ADD CONSTRAINT "CareSchedules_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4450 (class 2606 OID 25712)
-- Name: CareSchedules CareSchedules_ExperimentStageId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CareSchedules"
    ADD CONSTRAINT "CareSchedules_ExperimentStageId_fkey" FOREIGN KEY ("ExperimentStageId") REFERENCES public."ExperimentStages"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4422 (class 2606 OID 25577)
-- Name: CropVarieties CropVarieties_CropId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."CropVarieties"
    ADD CONSTRAINT "CropVarieties_CropId_fkey" FOREIGN KEY ("CropId") REFERENCES public."Crops"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4442 (class 2606 OID 25682)
-- Name: ExperimentBedAssignments ExperimentBedAssignments_BedId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentBedAssignments"
    ADD CONSTRAINT "ExperimentBedAssignments_BedId_fkey" FOREIGN KEY ("BedId") REFERENCES public."Beds"("Id") ON DELETE RESTRICT DEFERRABLE;


--
-- TOC entry 4443 (class 2606 OID 25677)
-- Name: ExperimentBedAssignments ExperimentBedAssignments_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentBedAssignments"
    ADD CONSTRAINT "ExperimentBedAssignments_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4438 (class 2606 OID 25657)
-- Name: ExperimentDesigns ExperimentDesigns_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentDesigns"
    ADD CONSTRAINT "ExperimentDesigns_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4439 (class 2606 OID 25662)
-- Name: ExperimentGroups ExperimentGroups_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentGroups"
    ADD CONSTRAINT "ExperimentGroups_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4488 (class 2606 OID 25912)
-- Name: ExperimentReports ExperimentReports_CreatedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentReports"
    ADD CONSTRAINT "ExperimentReports_CreatedBy_fkey" FOREIGN KEY ("CreatedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4489 (class 2606 OID 25907)
-- Name: ExperimentReports ExperimentReports_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentReports"
    ADD CONSTRAINT "ExperimentReports_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4426 (class 2606 OID 25607)
-- Name: ExperimentRequests ExperimentRequests_CropVarietyId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentRequests"
    ADD CONSTRAINT "ExperimentRequests_CropVarietyId_fkey" FOREIGN KEY ("CropVarietyId") REFERENCES public."CropVarieties"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4427 (class 2606 OID 25597)
-- Name: ExperimentRequests ExperimentRequests_FarmId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentRequests"
    ADD CONSTRAINT "ExperimentRequests_FarmId_fkey" FOREIGN KEY ("FarmId") REFERENCES public."Farms"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4428 (class 2606 OID 25612)
-- Name: ExperimentRequests ExperimentRequests_ProcedureTemplateId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentRequests"
    ADD CONSTRAINT "ExperimentRequests_ProcedureTemplateId_fkey" FOREIGN KEY ("ProcedureTemplateId") REFERENCES public."ProcedureTemplates"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4429 (class 2606 OID 25602)
-- Name: ExperimentRequests ExperimentRequests_ResearcherId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentRequests"
    ADD CONSTRAINT "ExperimentRequests_ResearcherId_fkey" FOREIGN KEY ("ResearcherId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4437 (class 2606 OID 25652)
-- Name: ExperimentStages ExperimentStages_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ExperimentStages"
    ADD CONSTRAINT "ExperimentStages_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4432 (class 2606 OID 25642)
-- Name: Experiments Experiments_CropVarietyId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_CropVarietyId_fkey" FOREIGN KEY ("CropVarietyId") REFERENCES public."CropVarieties"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4433 (class 2606 OID 25632)
-- Name: Experiments Experiments_FarmId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_FarmId_fkey" FOREIGN KEY ("FarmId") REFERENCES public."Farms"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4434 (class 2606 OID 25647)
-- Name: Experiments Experiments_ProcedureTemplateId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_ProcedureTemplateId_fkey" FOREIGN KEY ("ProcedureTemplateId") REFERENCES public."ProcedureTemplates"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4435 (class 2606 OID 25627)
-- Name: Experiments Experiments_RequestId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_RequestId_fkey" FOREIGN KEY ("RequestId") REFERENCES public."ExperimentRequests"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4436 (class 2606 OID 25637)
-- Name: Experiments Experiments_ResearcherId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Experiments"
    ADD CONSTRAINT "Experiments_ResearcherId_fkey" FOREIGN KEY ("ResearcherId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4419 (class 2606 OID 25562)
-- Name: Farms Farms_ManagerId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Farms"
    ADD CONSTRAINT "Farms_ManagerId_fkey" FOREIGN KEY ("ManagerId") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4492 (class 2606 OID 25927)
-- Name: KnowledgeDocumentChunks KnowledgeDocumentChunks_DocumentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."KnowledgeDocumentChunks"
    ADD CONSTRAINT "KnowledgeDocumentChunks_DocumentId_fkey" FOREIGN KEY ("DocumentId") REFERENCES public."KnowledgeDocuments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4490 (class 2606 OID 25917)
-- Name: KnowledgeDocuments KnowledgeDocuments_CropVarietyId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."KnowledgeDocuments"
    ADD CONSTRAINT "KnowledgeDocuments_CropVarietyId_fkey" FOREIGN KEY ("CropVarietyId") REFERENCES public."CropVarieties"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4491 (class 2606 OID 25922)
-- Name: KnowledgeDocuments KnowledgeDocuments_UploadedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."KnowledgeDocuments"
    ADD CONSTRAINT "KnowledgeDocuments_UploadedBy_fkey" FOREIGN KEY ("UploadedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4440 (class 2606 OID 25667)
-- Name: MeasurementDefinitions MeasurementDefinitions_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementDefinitions"
    ADD CONSTRAINT "MeasurementDefinitions_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4441 (class 2606 OID 25672)
-- Name: MeasurementDefinitions MeasurementDefinitions_GroupId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementDefinitions"
    ADD CONSTRAINT "MeasurementDefinitions_GroupId_fkey" FOREIGN KEY ("GroupId") REFERENCES public."ExperimentGroups"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4464 (class 2606 OID 25797)
-- Name: MeasurementRecords MeasurementRecords_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4465 (class 2606 OID 25787)
-- Name: MeasurementRecords MeasurementRecords_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4466 (class 2606 OID 25792)
-- Name: MeasurementRecords MeasurementRecords_ExperimentStageId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_ExperimentStageId_fkey" FOREIGN KEY ("ExperimentStageId") REFERENCES public."ExperimentStages"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4467 (class 2606 OID 25807)
-- Name: MeasurementRecords MeasurementRecords_MeasuredBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_MeasuredBy_fkey" FOREIGN KEY ("MeasuredBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4468 (class 2606 OID 25802)
-- Name: MeasurementRecords MeasurementRecords_MeasurementDefinitionId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."MeasurementRecords"
    ADD CONSTRAINT "MeasurementRecords_MeasurementDefinitionId_fkey" FOREIGN KEY ("MeasurementDefinitionId") REFERENCES public."MeasurementDefinitions"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4494 (class 2606 OID 25973)
-- Name: Notifications Notifications_RecipientId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "Notifications_RecipientId_fkey" FOREIGN KEY ("RecipientId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- TOC entry 4495 (class 2606 OID 25978)
-- Name: Notifications Notifications_SenderId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "Notifications_SenderId_fkey" FOREIGN KEY ("SenderId") REFERENCES public."Users"("Id") ON DELETE SET NULL;


--
-- TOC entry 4473 (class 2606 OID 25847)
-- Name: PlantHealthAssessments PlantHealthAssessments_AssessedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantHealthAssessments"
    ADD CONSTRAINT "PlantHealthAssessments_AssessedBy_fkey" FOREIGN KEY ("AssessedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4474 (class 2606 OID 25837)
-- Name: PlantHealthAssessments PlantHealthAssessments_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantHealthAssessments"
    ADD CONSTRAINT "PlantHealthAssessments_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4475 (class 2606 OID 25832)
-- Name: PlantHealthAssessments PlantHealthAssessments_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantHealthAssessments"
    ADD CONSTRAINT "PlantHealthAssessments_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4476 (class 2606 OID 25842)
-- Name: PlantHealthAssessments PlantHealthAssessments_ImageId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantHealthAssessments"
    ADD CONSTRAINT "PlantHealthAssessments_ImageId_fkey" FOREIGN KEY ("ImageId") REFERENCES public."PlantImages"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4469 (class 2606 OID 25817)
-- Name: PlantImages PlantImages_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantImages"
    ADD CONSTRAINT "PlantImages_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4470 (class 2606 OID 25812)
-- Name: PlantImages PlantImages_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantImages"
    ADD CONSTRAINT "PlantImages_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4471 (class 2606 OID 25822)
-- Name: PlantImages PlantImages_TaskReportId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantImages"
    ADD CONSTRAINT "PlantImages_TaskReportId_fkey" FOREIGN KEY ("TaskReportId") REFERENCES public."TaskReports"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4472 (class 2606 OID 25827)
-- Name: PlantImages PlantImages_UploadedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."PlantImages"
    ADD CONSTRAINT "PlantImages_UploadedBy_fkey" FOREIGN KEY ("UploadedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4425 (class 2606 OID 25592)
-- Name: ProcedureTemplateSteps ProcedureTemplateSteps_TemplateId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ProcedureTemplateSteps"
    ADD CONSTRAINT "ProcedureTemplateSteps_TemplateId_fkey" FOREIGN KEY ("TemplateId") REFERENCES public."ProcedureTemplates"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4423 (class 2606 OID 25587)
-- Name: ProcedureTemplates ProcedureTemplates_CreatedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ProcedureTemplates"
    ADD CONSTRAINT "ProcedureTemplates_CreatedBy_fkey" FOREIGN KEY ("CreatedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4424 (class 2606 OID 25582)
-- Name: ProcedureTemplates ProcedureTemplates_CropVarietyId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."ProcedureTemplates"
    ADD CONSTRAINT "ProcedureTemplates_CropVarietyId_fkey" FOREIGN KEY ("CropVarietyId") REFERENCES public."CropVarieties"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4430 (class 2606 OID 25617)
-- Name: RequestReviews RequestReviews_RequestId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."RequestReviews"
    ADD CONSTRAINT "RequestReviews_RequestId_fkey" FOREIGN KEY ("RequestId") REFERENCES public."ExperimentRequests"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4431 (class 2606 OID 25622)
-- Name: RequestReviews RequestReviews_ReviewerId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."RequestReviews"
    ADD CONSTRAINT "RequestReviews_ReviewerId_fkey" FOREIGN KEY ("ReviewerId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4480 (class 2606 OID 25877)
-- Name: SensorData SensorData_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorData"
    ADD CONSTRAINT "SensorData_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4481 (class 2606 OID 25872)
-- Name: SensorData SensorData_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorData"
    ADD CONSTRAINT "SensorData_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4482 (class 2606 OID 25867)
-- Name: SensorData SensorData_SensorId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorData"
    ADD CONSTRAINT "SensorData_SensorId_fkey" FOREIGN KEY ("SensorId") REFERENCES public."Sensors"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4483 (class 2606 OID 25887)
-- Name: SensorThresholdRules SensorThresholdRules_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorThresholdRules"
    ADD CONSTRAINT "SensorThresholdRules_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4484 (class 2606 OID 25882)
-- Name: SensorThresholdRules SensorThresholdRules_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SensorThresholdRules"
    ADD CONSTRAINT "SensorThresholdRules_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4493 (class 2606 OID 25946)
-- Name: SystemLogs SystemLogs_UserId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."SystemLogs"
    ADD CONSTRAINT "SystemLogs_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE SET NULL;


--
-- TOC entry 4459 (class 2606 OID 25772)
-- Name: TaskAssignments TaskAssignments_AssignedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskAssignments"
    ADD CONSTRAINT "TaskAssignments_AssignedBy_fkey" FOREIGN KEY ("AssignedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4460 (class 2606 OID 25767)
-- Name: TaskAssignments TaskAssignments_AssigneeId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskAssignments"
    ADD CONSTRAINT "TaskAssignments_AssigneeId_fkey" FOREIGN KEY ("AssigneeId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4461 (class 2606 OID 25762)
-- Name: TaskAssignments TaskAssignments_TaskId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskAssignments"
    ADD CONSTRAINT "TaskAssignments_TaskId_fkey" FOREIGN KEY ("TaskId") REFERENCES public."Tasks"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4462 (class 2606 OID 25782)
-- Name: TaskReports TaskReports_ReporterId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskReports"
    ADD CONSTRAINT "TaskReports_ReporterId_fkey" FOREIGN KEY ("ReporterId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4463 (class 2606 OID 25777)
-- Name: TaskReports TaskReports_TaskId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskReports"
    ADD CONSTRAINT "TaskReports_TaskId_fkey" FOREIGN KEY ("TaskId") REFERENCES public."Tasks"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4457 (class 2606 OID 25757)
-- Name: TaskSkillRequirements TaskSkillRequirements_SkillId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskSkillRequirements"
    ADD CONSTRAINT "TaskSkillRequirements_SkillId_fkey" FOREIGN KEY ("SkillId") REFERENCES public."Skills"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4458 (class 2606 OID 25752)
-- Name: TaskSkillRequirements TaskSkillRequirements_TaskId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."TaskSkillRequirements"
    ADD CONSTRAINT "TaskSkillRequirements_TaskId_fkey" FOREIGN KEY ("TaskId") REFERENCES public."Tasks"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4451 (class 2606 OID 25747)
-- Name: Tasks Tasks_AssignedTo_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_AssignedTo_fkey" FOREIGN KEY ("AssignedTo") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4452 (class 2606 OID 25732)
-- Name: Tasks Tasks_BatchId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_BatchId_fkey" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4453 (class 2606 OID 25737)
-- Name: Tasks Tasks_CareScheduleId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_CareScheduleId_fkey" FOREIGN KEY ("CareScheduleId") REFERENCES public."CareSchedules"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4454 (class 2606 OID 25742)
-- Name: Tasks Tasks_CreatedBy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_CreatedBy_fkey" FOREIGN KEY ("CreatedBy") REFERENCES public."Users"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4455 (class 2606 OID 25722)
-- Name: Tasks Tasks_ExperimentId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_ExperimentId_fkey" FOREIGN KEY ("ExperimentId") REFERENCES public."Experiments"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4456 (class 2606 OID 25727)
-- Name: Tasks Tasks_ExperimentStageId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."Tasks"
    ADD CONSTRAINT "Tasks_ExperimentStageId_fkey" FOREIGN KEY ("ExperimentStageId") REFERENCES public."ExperimentStages"("Id") ON DELETE SET NULL DEFERRABLE;


--
-- TOC entry 4415 (class 2606 OID 25547)
-- Name: UserRoles UserRoles_RoleId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "UserRoles_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4416 (class 2606 OID 25542)
-- Name: UserRoles UserRoles_UserId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "UserRoles_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4417 (class 2606 OID 25557)
-- Name: UserSkills UserSkills_SkillId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserSkills"
    ADD CONSTRAINT "UserSkills_SkillId_fkey" FOREIGN KEY ("SkillId") REFERENCES public."Skills"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4418 (class 2606 OID 25552)
-- Name: UserSkills UserSkills_UserId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: SEP490_Smartfarm_DB
--

ALTER TABLE ONLY public."UserSkills"
    ADD CONSTRAINT "UserSkills_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE DEFERRABLE;


--
-- TOC entry 4687 (class 0 OID 0)
-- Dependencies: 269
-- Name: FUNCTION pg_replication_origin_advance(text, pg_lsn); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_advance(text, pg_lsn) TO azure_pg_admin;


--
-- TOC entry 4688 (class 0 OID 0)
-- Dependencies: 261
-- Name: FUNCTION pg_replication_origin_create(text); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_create(text) TO azure_pg_admin;


--
-- TOC entry 4689 (class 0 OID 0)
-- Dependencies: 262
-- Name: FUNCTION pg_replication_origin_drop(text); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_drop(text) TO azure_pg_admin;


--
-- TOC entry 4690 (class 0 OID 0)
-- Dependencies: 270
-- Name: FUNCTION pg_replication_origin_oid(text); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_oid(text) TO azure_pg_admin;


--
-- TOC entry 4691 (class 0 OID 0)
-- Dependencies: 263
-- Name: FUNCTION pg_replication_origin_progress(text, boolean); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_progress(text, boolean) TO azure_pg_admin;


--
-- TOC entry 4692 (class 0 OID 0)
-- Dependencies: 264
-- Name: FUNCTION pg_replication_origin_session_is_setup(); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_session_is_setup() TO azure_pg_admin;


--
-- TOC entry 4693 (class 0 OID 0)
-- Dependencies: 265
-- Name: FUNCTION pg_replication_origin_session_progress(boolean); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_session_progress(boolean) TO azure_pg_admin;


--
-- TOC entry 4694 (class 0 OID 0)
-- Dependencies: 266
-- Name: FUNCTION pg_replication_origin_session_reset(); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_session_reset() TO azure_pg_admin;


--
-- TOC entry 4695 (class 0 OID 0)
-- Dependencies: 267
-- Name: FUNCTION pg_replication_origin_session_setup(text); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_session_setup(text) TO azure_pg_admin;


--
-- TOC entry 4696 (class 0 OID 0)
-- Dependencies: 271
-- Name: FUNCTION pg_replication_origin_xact_reset(); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_xact_reset() TO azure_pg_admin;


--
-- TOC entry 4697 (class 0 OID 0)
-- Dependencies: 268
-- Name: FUNCTION pg_replication_origin_xact_setup(pg_lsn, timestamp with time zone); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_replication_origin_xact_setup(pg_lsn, timestamp with time zone) TO azure_pg_admin;


--
-- TOC entry 4698 (class 0 OID 0)
-- Dependencies: 272
-- Name: FUNCTION pg_show_replication_origin_status(OUT local_id oid, OUT external_id text, OUT remote_lsn pg_lsn, OUT local_lsn pg_lsn); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_show_replication_origin_status(OUT local_id oid, OUT external_id text, OUT remote_lsn pg_lsn, OUT local_lsn pg_lsn) TO azure_pg_admin;


--
-- TOC entry 4699 (class 0 OID 0)
-- Dependencies: 258
-- Name: FUNCTION pg_stat_reset(); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_stat_reset() TO azure_pg_admin;


--
-- TOC entry 4700 (class 0 OID 0)
-- Dependencies: 273
-- Name: FUNCTION pg_stat_reset_shared(target text); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_stat_reset_shared(target text) TO azure_pg_admin;


--
-- TOC entry 4701 (class 0 OID 0)
-- Dependencies: 260
-- Name: FUNCTION pg_stat_reset_single_function_counters(oid); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_stat_reset_single_function_counters(oid) TO azure_pg_admin;


--
-- TOC entry 4702 (class 0 OID 0)
-- Dependencies: 259
-- Name: FUNCTION pg_stat_reset_single_table_counters(oid); Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT ALL ON FUNCTION pg_catalog.pg_stat_reset_single_table_counters(oid) TO azure_pg_admin;


--
-- TOC entry 4703 (class 0 OID 0)
-- Dependencies: 98
-- Name: COLUMN pg_config.name; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(name) ON TABLE pg_catalog.pg_config TO azure_pg_admin;


--
-- TOC entry 4704 (class 0 OID 0)
-- Dependencies: 98
-- Name: COLUMN pg_config.setting; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(setting) ON TABLE pg_catalog.pg_config TO azure_pg_admin;


--
-- TOC entry 4705 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.line_number; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(line_number) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4706 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.type; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(type) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4707 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.database; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(database) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4708 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.user_name; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(user_name) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4709 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.address; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(address) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4710 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.netmask; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(netmask) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4711 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.auth_method; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(auth_method) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4712 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.options; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(options) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4713 (class 0 OID 0)
-- Dependencies: 94
-- Name: COLUMN pg_hba_file_rules.error; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(error) ON TABLE pg_catalog.pg_hba_file_rules TO azure_pg_admin;


--
-- TOC entry 4714 (class 0 OID 0)
-- Dependencies: 146
-- Name: COLUMN pg_replication_origin_status.local_id; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(local_id) ON TABLE pg_catalog.pg_replication_origin_status TO azure_pg_admin;


--
-- TOC entry 4715 (class 0 OID 0)
-- Dependencies: 146
-- Name: COLUMN pg_replication_origin_status.external_id; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(external_id) ON TABLE pg_catalog.pg_replication_origin_status TO azure_pg_admin;


--
-- TOC entry 4716 (class 0 OID 0)
-- Dependencies: 146
-- Name: COLUMN pg_replication_origin_status.remote_lsn; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(remote_lsn) ON TABLE pg_catalog.pg_replication_origin_status TO azure_pg_admin;


--
-- TOC entry 4717 (class 0 OID 0)
-- Dependencies: 146
-- Name: COLUMN pg_replication_origin_status.local_lsn; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(local_lsn) ON TABLE pg_catalog.pg_replication_origin_status TO azure_pg_admin;


--
-- TOC entry 4718 (class 0 OID 0)
-- Dependencies: 99
-- Name: COLUMN pg_shmem_allocations.name; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(name) ON TABLE pg_catalog.pg_shmem_allocations TO azure_pg_admin;


--
-- TOC entry 4719 (class 0 OID 0)
-- Dependencies: 99
-- Name: COLUMN pg_shmem_allocations.off; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(off) ON TABLE pg_catalog.pg_shmem_allocations TO azure_pg_admin;


--
-- TOC entry 4720 (class 0 OID 0)
-- Dependencies: 99
-- Name: COLUMN pg_shmem_allocations.size; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(size) ON TABLE pg_catalog.pg_shmem_allocations TO azure_pg_admin;


--
-- TOC entry 4721 (class 0 OID 0)
-- Dependencies: 99
-- Name: COLUMN pg_shmem_allocations.allocated_size; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(allocated_size) ON TABLE pg_catalog.pg_shmem_allocations TO azure_pg_admin;


--
-- TOC entry 4722 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.starelid; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(starelid) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4723 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staattnum; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staattnum) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4724 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stainherit; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stainherit) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4725 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanullfrac; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanullfrac) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4726 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stawidth; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stawidth) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4727 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stadistinct; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stadistinct) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4728 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stakind1; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stakind1) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4729 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stakind2; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stakind2) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4730 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stakind3; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stakind3) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4731 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stakind4; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stakind4) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4732 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stakind5; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stakind5) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4733 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staop1; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staop1) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4734 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staop2; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staop2) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4735 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staop3; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staop3) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4736 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staop4; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staop4) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4737 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.staop5; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(staop5) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4738 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stacoll1; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stacoll1) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4739 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stacoll2; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stacoll2) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4740 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stacoll3; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stacoll3) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4741 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stacoll4; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stacoll4) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4742 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stacoll5; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stacoll5) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4743 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanumbers1; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanumbers1) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4744 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanumbers2; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanumbers2) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4745 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanumbers3; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanumbers3) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4746 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanumbers4; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanumbers4) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4747 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stanumbers5; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stanumbers5) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4748 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stavalues1; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stavalues1) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4749 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stavalues2; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stavalues2) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4750 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stavalues3; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stavalues3) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4751 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stavalues4; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stavalues4) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4752 (class 0 OID 0)
-- Dependencies: 39
-- Name: COLUMN pg_statistic.stavalues5; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(stavalues5) ON TABLE pg_catalog.pg_statistic TO azure_pg_admin;


--
-- TOC entry 4753 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.oid; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(oid) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4754 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subdbid; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subdbid) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4755 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subname; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subname) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4756 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subowner; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subowner) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4757 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subenabled; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subenabled) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4758 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subconninfo; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subconninfo) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4759 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subslotname; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subslotname) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4760 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subsynccommit; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subsynccommit) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


--
-- TOC entry 4761 (class 0 OID 0)
-- Dependencies: 64
-- Name: COLUMN pg_subscription.subpublications; Type: ACL; Schema: pg_catalog; Owner: azuresu
--

GRANT SELECT(subpublications) ON TABLE pg_catalog.pg_subscription TO azure_pg_admin;


-- Completed on 2026-06-20 12:37:09

--
-- PostgreSQL database dump complete
--

\unrestrict mBZvYuilaThNNTjo4j92xxqp2Xbs9aXHYilWWKtJcLLIjCRyWz28OtrrfbO4ega

