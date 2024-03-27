using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLReaderTool
{
    public readonly struct BoundingBox
    {
        public BoundingBox(Tuple<double, double> suedwestecke, Tuple<double, double> nordostecke)
        {
            Suedwestecke = suedwestecke;
            Nordostecke = nordostecke;
        }
        /*public BoundingBox(Tuple<double, double> zentrum, double nordSuedInKm, double ostWestInKm)
        {
            // Folgende Gleichung ist nach x umzustellen und dann der naechstgelegene Wert zu nehmen (Periodizität!)
            1/2*nordsuedInKm = Math.Sqrt((Math.Cos(x)-Math.Cos(zentrum.Item1))^2 + (Math.Sin(x)-Math.Sin(zentrum.Item1))^2)
        }*/
        public Tuple<double, double> Suedwestecke { get; }
        public Tuple<double, double> Nordostecke { get; }
        public override string ToString() => $"({Suedwestecke}, {Nordostecke})";
    };

    internal class Program
    {
        //Dient dazu, gezielt alle Tags auszuschließen, die nicht berücksichtigt werden SOLLEN, damit nur unbekannte geloogt werden
        // und anschließend über den Umgang mit diesen entschieden werden kann.
        private static readonly List<string> IrrelevanteTags = new List<string>()
                {
                    "k incline",
                    "k ref",
                    "k surface",
                    "k electrified",
                    "k frequency",
                    "k gauge",
                    "k operator",
                    "k operator:wikidata",
                    "k passenger_lines",
                    "k railway",
                    "k railway:etcs",
                    "k railway:gnt",
                    "k railway:lzb",
                    "k railway:pzb",
                    "k usage",
                    "k voltage",
                    "k workrules",
                    "k lanes",
                    "k lit",
                    "k maxheight",
                    "k bridge",
                    "k destination:ref",
                    "k layer",
                    "k destination",
                    "k maxspeed:type",
                    "k turn:lanes",
                    "k loc_ref",
                    "k maxspeed:conditional",
                    "k destination:colour:lanes",
                    "k destination:lanes",
                    "k destination:ref:lanes",
                    "k destination:ref:to:lanes",
                    "k sidewalk:right:surface",
                    "k smoothness",
                    "k lanes:backward",
                    "k lanes:forward",
                    "k sidewalk:both:surface",
                    "k source:position",
                    "k zone:traffic",
                    "k lane_markings",
                    "k bus:lanes",
                    "k check_date",
                    "k maxlength",
                    "k postal_code",
                    "k sidewalk:left:surface",
                    "k source:width",
                    "k width",
                    "k turn:lanes:forward",
                    "k maxweight:signed",
                    "k overtaking:hgv",
                    "k placement",
                    "k wikidata",
                    "k wikipedia",
                    "k turn:lanes:backward",
                    "k psv:lanes:forward",
                    "k zone:maxspeed",
                    "k note:de",
                    "k check_date:maxspeed",
                    "k note",
                    "k source:maxspeed",
                    "k check_date:cycleway",
                    "k change:lanes:backward",
                    "k change:lanes:forward",
                    "k moped",
                    "k motorroad",
                    "k source",
                    "k maxweight",
                    "k placement:forward",
                    "k tracktype",
                    "k destination:colour:lanes:backward",
                    "k destination:lanes:backward",
                    "k destination:ref:to:lanes:backward",
                    "k destination:to:lanes:backward",
                    "k emergency",
                    "k psv",
                    "k hgv:forward",
                    "k abandoned:highway",
                    "k abandoned:surface",
                    "k abandoned:tracktype",
                    "k abandoned:width",
                    "k access",
                    "k motorcycle",
                    "k lanes:bus:backward",
                    "k psv:lanes:backward",
                    "k vehicle:lanes",
                    "k destination:colour:backward",
                    "k destination:symbol:lanes:backward",
                    "k traffic_sign",
                    "k change:lanes",
                    "k oneway:psv",
                    "k horse",
                    "k natural",
                    "k old_name",
                    "k bridge:name",
                    "k length",
                    "k loc_name",
                    "k landuse",
                    "k ownership",
                    "k level",
                    "k railway:bidirectional",
                    "k railway:radio",
                    "k description",
                    "k hgv",
                    "k junction:ref",
                    "k cycleway:note",
                    "k destination:ref:lanes:backward",
                    "k denomination",
                    "k religion",
                    "k tunnel",
                    "k reg_name",
                    "k maxweight:bus",
                    "k mtb:scale",
                    "k amenity",
                    "k contact:email",
                    "k contact:fax",
                    "k contact:phone",
                    "k contact:website",
                    "k full_name",
                    "k fee",
                    "k parking",
                    "k lit:check_date",
                    "k junction",
                    "k bicycle:conditional",
                    "k maxweightrating",
                    "k vehicle:conditional",
                    "k abandoned:name",
                    "k abandoned:railway",
                    "k abandoned:ref",
                    "k class:bicycle",
                    "k cycleway:surface",
                    "k footway:surface",
                    "k official_name",
                    "k segregated",
                    "k check_date:surface",
                    "k cycleway:width",
                    "k placement:backward",
                    "k overtaking",
                    "k sidewalk:left:winter_service",
                    "k area",
                    "k de:strassenschluessel_exists",
                    "k operator:type",
                    "k source:name",
                    "k traffic_calming",
                    "k name:etymology:wikidata",
                    "k service",
                    "k leisure",
                    "k sport",
                    "k max_age",
                    "k playground:theme",
                    "k waterway",
                    "k addr:city",
                    "k addr:country",
                    "k addr:housename",
                    "k addr:housenumber",
                    "k addr:postcode",
                    "k addr:street",
                    "k architect",
                    "k architect:wikidata",
                    "k historic",
                    "k start_date",
                    "k internet_access",
                    "k phone",
                    "k short_name",
                    "k toilets:wheelchair",
                    "k website",
                    "k wheelchair",
                    "k wheelchair:description",
                    "k bus:lanes:forward",
                    "k vehicle:lanes:forward",
                    "k construction",
                    "k construction_end_expected",
                    "k construction_start_expected",
                    "k narrow",
                    "k opening_date",
                    "k noname",
                    "k destination:arrow:lanes:forward",
                    "k destination:colour:lanes:forward",
                    "k destination:lanes:forward",
                    "k destination:ref:lanes:forward",
                    "k destination:ref:to:lanes:forward",
                    "k isced:level",
                    "k addr:suburb",
                    "k capacity",
                    "k capacity:disabled",
                    "k park_ride",
                    "k was:amenity",
                    "k was:capacity",
                    "k was:charge",
                    "k was:fee",
                    "k was:fee:conditional",
                    "k was:parking",
                    "k was:payment:cards",
                    "k was:wheelchair",
                    "k avalanche_protector:left",
                    "k covered",
                    "k abandoned:amenity",
                    "k abandoned:sport",
                    "k demolished:building",
                    "k check_date:fee",
                    "k turn",
                    "k bettundbike",
                    "k brand",
                    "k brand:wikidata",
                    "k brand:wikipedia",
                    "k garden:type",
                    "k alt_name",
                    "k healthcare",
                    "k operator:wikipedia",
                    "k handrail",
                    "k ramp",
                    "k step_count",
                    "k trail_visibility",
                    "k motor_vehicle:backward:conditional",
                    "k winter_service",
                    "k maxspeed:source",
                    "k disused:power",
                    "k architect:wikipedia",
                    "k maxspeed:reason",
                    "k heritage",
                    "k heritage:operator",
                    "k heritage:ref",
                    "k heritage:website",
                    "k parking:both",
                    "k parking:both:orientation",
                    "k parking:left:orientation",
                    "k parking:right:orientation",
                    "k traffic_sign:forward",
                    "k leaf_type",
                    "k boat",
                    "k mofa",
                    "k created_by",
                    "k motor_vehicle:conditional",
                    "k indoor",
                    "k handrail:left",
                    "k handrail:right",
                    "k tactile_paving",
                    "k lanes:psv",
                    "k maxweight:hgv",
                    "k check_date:lit",
                    "k disused:leisure",
                    "k leaf_cycle",
                    "k traffic_sign:backward",
                    "k place",
                    "k snowmobile",
                    "k dog",
                    "k maxstay",
                    "k abandoned:electrified",
                    "k abandoned:frequency",
                    "k abandoned:gauge",
                    "k abandoned:rack",
                    "k abandoned:track",
                    "k abandoned:usage",
                    "k abandoned:voltage",
                    "k razed:railway",
                    "k fixme",
                    "k sac_scale",
                    "k supervised",
                    "k historic:railway",
                    "k ramp:bicycle",
                    "k hgv:maxlength",
                    "k railway:track_ref",
                    "k ramp:stroller",
                    "k embankment",
                    "k health_facility:type",
                    "k health_person:type",
                    "k health_specialty:psychiatry",
                    "k healthcare:speciality",
                    "k removed:highway",
                    "k barrier",
                    "k surface:left",
                    "k surface:middle",
                    "k surface:right",
                    "k surface:note",
                    "k water",
                    "k power",
                    "k substation",
                    "k informal",
                    "k stroller",
                    "k source:maxspeed:conditional",
                    "k parking:condition:right",
                    "k parking:lane:left",
                    "k last_renovation",
                    "k sidewalk:both:smoothness",
                    "k construction:start_date",
                    "k priority_road",
                    "k location",
                    "k survey:date",
                    "k maxactualweight",
                    "k destination:colour",
                    "k destination:symbol:lanes",
                    "k psv:lanes",
                    "k parking:lane:both",
                    "k lanes:psv:backward",
                    "k lanes:psv:forward",
                    "k long_name",
                    "k long_name:ar",
                    "k monorail",
                    "k name:ar",
                    "k tracks",
                    "k url",
                    "k maxheight:physical",
                    "k tunnel:name",
                    "k tunnel:wikipedia",
                    "k wikimedia_commons",
                    "k blind:description:de",
                    "k inline_skates",
                    "k parking:condition:both",
                    "k parking:lane:left:diagonal",
                    "k parking:lane:left:marked",
                    "k parking:lane:right",
                    "k parking:lane:right:marked",
                    "k parking:lane:right:parallel",
                    "k bus",
                    "k parking:lane:left:parallel",
                    "k opening_hours",
                    "k tourism",
                    "k lanes:bus",
                    "k cemetery",
                    "k parking:lane:both:parallel",
                    "k surface:cycleway",
                    "k surface:footway",
                    "k email",
                    "k fax",
                    "k lunch",
                    "k school",
                    "k heritage:since",
                    "k plots",
                    "k ramp:wheelchair",
                    "k check_date:step_count",
                    "k tactile_writing:braille:de",
                    "k tactile_writing:embossed_printed_letters:de",
                    "k bus:lanes:backward",
                    "k mapillary",
                    "k crop",
                    "k landing",
                    "k image",
                    "k year_of_construction",
                    "k maxwidth",
                    "k bett_and_bike:ref",
                    "k pvs",
                    "k addr:unit",
                    "k building:part",
                    "k toll",
                    "k mtb:scale:uphill",
                    "k maxweightrating:hgv",
                    "k steps",
                    "k mofa:forward:conditional",
                    "k moped:forward:conditional",
                    "k motorcycle:forward:conditional",
                    "k ref:Wuppertal",
                    "k wall",
                    "k man_made",
                    "k cables",
                    "k circuits",
                    "k wires",
                    "k name:etymology",
                    "k max_clothing",
                    "k nudism",
                    "k automated",
                    "k atm",
                    "k bic",
                    "k asb",
                    "k basilica",
                    "k basin",
                    "k intermittent",
                    "k short_name_1",
                    "k parking_lane",
                    "k building:levels",
                    "k vehicle:lanes:backward",
                    "k landcover",
                    "k addr:subdistrict",
                    "k abandoned:building",
                    "k area:highway",
                    "k abandoned:bunker_type",
                    "k abandoned:military",
                    "k abandoned:subtype",
                    "k hgv:conditional",
                    "k smoking",
                    "k fuel:GTL_diesel",
                    "k fuel:cng",
                    "k fuel:diesel",
                    "k fuel:lpg",
                    "k fuel:octane_100",
                    "k fuel:octane_95",
                    "k fuel:octane_98",
                    "k living_street",
                    "k railway:preferred_direction",
                    "k surface:check_date",
                    "k destination:symbol:to",
                    "k branch",
                    "k destination:ref:to",
                    "k capacity:car_sharing",
                    "k capacity:charging",
                    "k name:de",
                    "k name:en",
                    "k official_name:en",
                    "k roof:shape",
                    "k roof:levels",
                    "k disused:gauge",
                    "k disused:railway",
                    "k disused:service",
                    "k ski",
                    "k handrail:center",
                    "k step.condition",
                    "k step.height",
                    "k step.length",
                    "k surface.material",
                    "k disused:access",
                    "k disused:amenity",
                    "k disused:description",
                    "k disused:parking",
                    "k charge",
                    "k name:uk",
                    "k fuel:e10",
                    "k shop",
                    "k recycling_type",
                    "k social_facility:for",
                    "k club",
                    "k compressed_air",
                    "k industrial",
                    "k maxstay:conditional",
                    "k est_width",
                    "k name:signed",
                    "k maxweight:conditional",
                    "k end_date",
                    "k removed:building",
                    "k access:conditional",
                    "k height",
                    "k social_facility",
                    "k beds",
                    "k breakfast:buffet",
                    "k dinner",
                    "k internet_access:fee",
                    "k opening_hours:reception",
                    "k rooms",
                    "k tower:type",
                    "k boundary",
                    "k geological",
                    "k protect_class",
                    "k type",
                    "k check_date:opening_hours",
                    "k opening_hours:signed",
                    "k FIXME",
                    "k operator:short",
                    "k maxlength:backward",
                    "k parking:left",
                    "k parking:left:fee",
                    "k parking:right",
                    "k hgv:backward",
                    "k short_name:ar",
                    "k meadow",
                    "k allotments",
                    "k check_date:sidewalk:surface",
                    "k mofa:conditional",
                    "k moped:conditional",
                    "k motorcycle:conditional",
                    "k capacity:women",
                    "k dogs",
                    "k swimming",
                    "k designation",
                    "k disused:public_transport",
                    "k disused:highway",
                    "k preserved:railway",
                    "k rack",
                    "k bench",
                    "k bin",
                    "k public_transport",
                    "k ref:IFOPT",
                    "k shelter",
                    "k addr:state",
                    "k abandoned",
                    "k local_ref",
                    "k network",
                    "k ref_name",
                    "k alt_name_1",
                    "k disused:operator",
                    "k bicycle:lanes",
                    "k destination:to:lanes",
                    "k material",
                    "k street_cabinet",
                    "k addr:place",
                    "k mtb:scale:imba",
                    "k name:wikipedia",
                    "k kerb",
                    "k bus_stop:info:vocal",
                    "k note1",
                    "k vrr:wabe",
                    "k taxi:lanes",
                    "k bridge:structure",
                    "k internet_access:operator",
                    "k internet_access:ssid",
                    "k removed:natural",
                    "k removed:water",
                    "k disused:ramp:luggage",
                    "k platform_lift",
                    "k ramp:luggage",
                    "k tactile_writing",
                    "k shelter_type",
                    "k clothes",
                    "k conveying",
                    "k duration",
                    "k check_date:bench",
                    "k source:foot",
                    "k destination:symbol:to:lanes:forward",
                    "k destination:symbol:to:lanes",
                    "k destination:arrow:lanes",
                    "k razed:building",
                    "k check_date:shelter",
                    "k ref:IBNR",
                    "k lanes:bus:forward",
                    "k check_date:tactile_paving",
                    "k ashtray",
                    "k air_conditioning",
                    "k building:colour",
                    "k building:min_level",
                    "k roof:colour",
                    "k building:material",
                    "k train",
                    "k maxspeed:lanes:backward",
                    "k psv:backward",
                    "k path",
                    "k building:underground",
                    "k abandoned:building:use",
                    "k abandoned:man_made",
                    "k note:name",
                    "k capacity:parent",
                    "k currency:EUR",
                    "k organic",
                    "k payment:american_express",
                    "k payment:apple_pay",
                    "k payment:cash",
                    "k payment:contactless",
                    "k payment:credit_cards",
                    "k payment:debit_cards",
                    "k payment:girocard",
                    "k payment:maestro",
                    "k payment:mastercard",
                    "k payment:visa",
                    "k tree_row:distance",
                    "k fence_type",
                    "k parking:condition",
                    "k parking:condition:default",
                    "k parking:condition:max_stay",
                    "k parking:condition:time_interval",
                    "k maxweightrating:conditional",
                    "k zoo",
                    "k old_railway_operator",
                    "k departures_board",
                    "k currency:others",
                    "k payment:coins",
                    "k payment:others",
                    "k cutting",
                    "k surface:colour",
                    "k surface:material",
                    "k operator:abbr",
                    "k note_1",
                    "k orientation",
                    "k elevator",
                    "k levelpart",
                    "k recycling:batteries",
                    "k recycling:car_batteries",
                    "k recycling:cardboard",
                    "k recycling:cartons",
                    "k recycling:clothes",
                    "k recycling:electrical_appliances",
                    "k recycling:electrical_items",
                    "k recycling:garden_waste",
                    "k recycling:glass",
                    "k recycling:glass_bottles",
                    "k recycling:green_waste",
                    "k recycling:magazines",
                    "k recycling:newspaper",
                    "k recycling:paper",
                    "k recycling:paper_packaging",
                    "k recycling:scrap_metal",
                    "k recycling:shoes",
                    "k recycling:small_appliances",
                    "k recycling:small_electrical_appliances",
                    "k recycling:styrofoam",
                    "k route_ref",
                    "k crossing",
                    "k crossing_ref",
                    "k proposed",
                    "k colour",
                    "k substance",
                    "k hoops",
                    "k hgv:semitrailer",
                    "k hgv:trailer",
                    "k turn:bus:lanes",
                    "k pole",
                    "k pole:check_date",
                    "k shelter:check_date",
                    "k razed:gauge",
                    "k construction:elevator",
                    "k proposed:landuse",
                    "k proposed:layer",
                    "k proposed:note",
                    "k attraction",
                    "k species:wikidata",
                    "k owner",
                    "k construction:amenity",
                    "k foot:note",
                    "k crossing:island",
                    "k crossing:markings",
                    "k source:outline",
                    "k recycling:cars",
                    "k roof:material",
                    "k emergency:social_facility",
                    "k source:geometry",
                    "k abandoned:service",
                    "k disused:bridge",
                    "k bollard_count",
                    "k building:levels:underground",
                    "k plant",
                    "k razed:service",
                    "k parking:condition:right:park_ride",
                    "k parking:lane",
                    "k repeat_on",
                    "k bridge:type",
                    "k bicycle_parking",
                    "k abandoned:leisure",
                    "k removed:bridge",
                    "k capacity:hgv",
                    "k BezirksRegierung:inscription_date",
                    "k check_date:handrail",
                    "k lanes:both_ways",
                    "k turn:lanes:both_ways",
                    "k check_date:ramp",
                    "k trailer",
                    "k abandoned:crossing",
                    "k school:de",
                    "k ref:street",
                    "k voltage:primary",
                    "k advertising",
                    "k animated",
                    "k direction",
                    "k faces",
                    "k luminous",
                    "k sides",
                    "k support",
                    "k maxweight:hgv:backward",
                    "k construction:note",
                    "k artist_name",
                    "k artwork_type",
                    "k taxi",
                    "k maxactualweight:bus",
                    "k playground",
                    "k government",
                    "k office",
                    "k disused:shop",
                    "k passenger_information_display",
                    "k kerb:approach_aid",
                    "k taxi:conditional",
                    "k brand:abbr",
                    "k passing_places",
                    "k payment:app",
                    "k payment:cards",
                    "k payment:notes",
                    "k playground:slide",
                    "k backrest",
                    "k object:city",
                    "k object:country",
                    "k object:housenumber",
                    "k object:postcode",
                    "k object:street",
                    "k residential",
                    "k craft",
                    "k ref:housenumber",
                    "k apartments:for",
                    "k denotation",
                    "k utility",
                    "k abandoned:bridge",
                    "k generator:method",
                    "k generator:source",
                    "k generator:type",
                    "k razed:highway",
                    "k protection_title",
                    "k ref:WDPA",
                    "k short_protection_title",
                    "k old_ref",
                    "k seasonal",
                    "k blind",
                    "k min_age",
                    "k access:disabled",
                    "k parking_space",
                    "k ref:DE-NW",
                    "k salt",
                    "k language:de",
                    "k language:fr",
                    "k bike_ride",
                    "k ford",
                    "k diameter",
                    "k format",
                    "k model_aerodrome",
                    "k model_aerodrome:combustion",
                    "k model_aerodrome:electric",
                    "k model_aerodrome:heli",
                    "k model_aerodrome:turbine",
                    "k memorial",
                    "k construction_date",
                    "k aeroway",
                    "k maxheight:signed",
                    "k college",
                    "k abandoned:power",
                    "k preschool",
                    "k parking:condition:left",
                    "k second_hand",
                    "k subject:wikidata",
                    "k subject:wikipedia",
                    "k cuisine",
                    "k delivery",
                    "k drive_through",
                    "k takeaway",
                    "k abandoned:shop",
                    "k deanery",
                    "k diocese",
                    "k parish",
                    "k service_times",
                    "k entrance",
                    "k razed",
                    "k outdoor_seating",
                    "k construction:name",
                    "k disused:name",
                    "k payment:diners_club",
                    "k payment:dkv",
                    "k payment:uta",
                    "k tenant",
                    "k boules",
                    "k payment:shell",
                    "k self_service",
                    "k generator:output:electricity",
                    "k community_centre",
                    "k phone:mobile",
                    "k demolished:power",
                    "k balcony",
                    "k community_centre:for",
                    "k roof:ridge",
                    "k roof:edge",
                    "k button_operated",
                    "k central_island:traversable",
                    "k contact:facebook",
                    "k contact:instagram",
                    "k diet:vegan",
                    "k diet:vegetarian",
                    "k indoor_seating",
                    "k indoor_seating:capacity",
                    "k outdoor_seating:capacity",
                    "k display",
                    "k speech_output:de",
                    "k line",
                    "k piste:type",
                    "k departures:bus",
                    "k departures:train",
                    "k departures_count",
                    "k model",
                    "k name",
                    "k building",
                    "k toilets",
                    "k building:architecture",
                    "k pets",
                    "k nohousenumber",
                    "k cycleway:segregated",
                    "k uic_ref",
                    "k ref:isil",
                    "k name_old",
                    "k disused:building",
                    "k service:vehicle:used_car_sales",
                    "k ele",
                    "k microbrewery",
                    "k ref:vatin",
                    "k courts",
                    "k pitches",
                    "k tower:construction",
                    "k rooms:disabled",
                    "k stars",
                    "k inscription",
                    "k source:height",
                    "k contact:youtube",
                    "k onkz",
                    "k telecom",
                    "k by:religious_title",
                    "k date:religious_title",
                    "k religious_title",
                    "k payment:paypal",
                    "k food",
                    "k service:vehicle:new_car_sales",
                    "k diet:halal",
                    "k diet:kosher",
                    "k payment:v_pay",
                    "k company",
                    "k bunker_type",
                    "k military",
                    "k cost",
                    "k building:use",
                    "k min_level",
                    "k surveillance",
                    "k reservation",
                    "k railway:ref",
                    "k payment:discover_card",
                    "k surveillance:type",
                    "k building:roof:levels",
                    "k source:addr:housenumber",
                    "k opening_hours:kitchen",
                    "k noaddress",
                    "k building:layer",
                    "k motorcycle:clothes",
                    "k motorcycle:parts",
                    "k disused:deanery",
                    "k disused:denomination",
                    "k disused:diocese",
                    "k disused:parish",
                    "k disused:religion",
                    "k name:ru",
                    "k int_name",
                    "k drink:wine",
                    "k building:name",
                    "k changing_table",
                    "k toilets:disposal",
                    "k toilets:position",
                    "k trade",
                    "k service:vehicle:body_repair",
                    "k service:vehicle:brakes",
                    "k service:vehicle:car_parts",
                    "k service:vehicle:car_repair",
                    "k service:vehicle:diagnostics",
                    "k service:vehicle:electrical",
                    "k service:vehicle:inspection",
                    "k service:vehicle:motor",
                    "k service:vehicle:oil_change",
                    "k service:vehicle:repairs",
                    "k service:vehicle:wheels",
                    "k roof:orientation",
                    "k fair_trade",
                    "k license_classes",
                    "k min_height",
                    "k opening_hours:covid19",
                    "k building:flats",
                    "k disused",
                    "k railway:local_operated",
                    "k railway:signal_box",
                    "k nonsquare",
                    "k disused:opening_hours",
                    "k disused:phone",
                    "k disused:website",
                    "k contact:mobile",
                    "k post_office",
                    "k post_office:brand",
                    "k post_office:brand:wikidata",
                    "k ruins",
                    "k building_1",
                    "k source_1",
                    "k number_of_apartments",
                    "k diet:gluten_free",
                    "k wheelchair:description:de",
                    "k museum",
                    "k toilets:access",
                    "k old_addr:housenumber",
                    "k old_addr:street",
                    "k service:vehicle:glass",
                    "k kiss_ride",
                    "k species:de",
                    "k lockable",
                    "k capacity:men",
                    "k female",
                    "k gender_segregated",
                    "k male",
                    "k toilets:handwashing",
                    "k sidewalk:left:traffic_sign"
                };
        private static BoundingBox Box_100 = new BoundingBox(new Tuple<double, double>(50.346226, 5.72198), new Tuple<double, double>(52.1452, 8.58682));

        private static void schreibeKnoten(XmlReader reader, FileStream ziel)
        {
            if (!reader.MoveToAttribute("id"))
            {
                Log($"Datenfehler: Knoten ohne ID gefunden!");
                return;
            }
            string knotenid = reader.Value;

            if (!reader.MoveToAttribute("lat"))
            {
                Log($"Datenfehler: Knoten {knotenid} ohne lat gefunden.");
                return;
            }
            double lat = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (lat <= Box_100.Suedwestecke.Item1 || lat >= Box_100.Nordostecke.Item1)
            {
                Log($"Knoten {knotenid} aussortiert wegen {lat}");
                return;
            }

            if (!reader.MoveToAttribute("lon"))
            {
                Log($"Datenfehler: Knoten {knotenid} ohne lon gefunden.");
                return;
            }
            double lon = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (lon <= Box_100.Suedwestecke.Item2 || lon >= Box_100.Nordostecke.Item2)
            {
                Log($"Knoten {knotenid} aussortiert wegen {lon}");
                return;
            }

            string toWrite = "n:\t" + knotenid + ":" + lat + ";" + lon + "\n";
            byte[] b = new UTF8Encoding(true).GetBytes(toWrite);
            ziel.Write(b, 0, b.Length);
            ziel.Flush();
        }

        private static double MAX_AUTO = 120d;
        private static double MAX_RAD = 25d;
        private static double MAX_FUSS = 5d;

        private static FileStream log = new FileStream(@".\exportlog.txt", FileMode.Create, FileAccess.ReadWrite);

        static void Main(string[] args)
        {

            XmlReaderSettings settings = new XmlReaderSettings();

            //using (XmlReader reader = XmlReader.Create(new FileStream(@"D:\nordrhein-westfalen-latest.osm", FileMode.Open), settings))
            using (XmlReader reader = XmlReader.Create(new FileStream(@"..\..\export.osm", FileMode.Open), settings))
            {
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                //FileStream ziel = new FileStream(@"D:\kartenausschnitt.dat", FileMode.Create, FileAccess.ReadWrite);
                //FileStream log = new FileStream(@"D:\log.dat", FileMode.Create, FileAccess.ReadWrite);
                FileStream ziel = new FileStream(@".\exported.txt", FileMode.Create, FileAccess.ReadWrite);
                int anzahlKnoten = 0;
                //Diese Liste dient dem Logging, um sicherzustellen, dass alle relevanten Tags berücksichtigt worden.
                List<string> unverarbeiteteTags = new List<string>();

                while (reader.Read())
                {
                    if (reader.Name.Equals("node") && reader.IsStartElement())
                    {
                        anzahlKnoten++;
                        schreibeKnoten(reader, ziel);
                    }
                    if (reader.Name.Equals("way") && reader.IsStartElement())
                    {
                        if (!reader.MoveToAttribute("id"))
                        {
                            Log("Datenfehler: Weg ohne ID gefunden.");
                            continue;
                        }
                        string wegid = reader.Value;

                        int suchtiefeKnoten = reader.Depth;

                        //Die vorlaeufige Liste enthält Knotenids
                        List<string> weg = new List<string>();
                        bool einbahnAuto = false;
                        bool einbahnRad = false;
                        bool explizitNichtEinbahnRad = false;
                        //Variablen, die angeben, ob der Weg (auch) andersherum befahrbar ist. Annahme: Fußgängereinbahnstraßen gibt es nicht,
                        //daher wird an den entsprechenden Stellen jeweils ein Fußweg in beide Richtungen angenommen. Genau genommen muss diese
                        //Information erst während Dijkstra angewendet werden, denn Dijkstra geht NIE rückwaärts.
                        bool isAndersherumAuto = true;
                        bool isAndersherumRad = true;
                        double maxspeedVorwaertsAuto = MAX_AUTO;
                        double maxspeedRueckwaertsAuto = MAX_AUTO;
                        double maxspeedVorwaertsRad = MAX_RAD;
                        double maxspeedRueckwaertsRad = MAX_RAD;
                        double maxspeedFuss = MAX_FUSS;
                        bool autoVorw = true;
                        bool autoRueckw = true;
                        bool radVorw = true;
                        bool radRueckw = true;
                        bool fuss = true;
                        bool autoVorwExplizitErlaubt = false;
                        bool autoRueckwExplizitErlaubt = false;
                        bool radVorwExplizitErlaubt = false;
                        bool radRueckwExplizitErlaubt = false;
                        bool fussExplizitErlaubt = false;
                        //Nur, wenn irgendwo ein Highway-Tag gefunden wurde, wird der Wert true und damit ein Weg geschrieben.
                        //Alle anderen "Wege" sind nur Linien, z.B. Gebaeude o.ä.
                        bool istWeg = false;

                        while (reader.Read() && reader.IsStartElement() && reader.Depth == suchtiefeKnoten)
                        {
                            if (reader.Name.Equals("nd"))
                            {
                                if (!reader.MoveToAttribute("ref"))
                                {
                                    Log("Datenfehler: Knoten ohne ref-Wert in Weg " + wegid + " gefunden.");
                                    continue;
                                }
                                weg.Add(reader.Value);
                                //Log($"{wegid} mit {weg.Count} Knoten gefunden");
                            }
                            // Dieser Abschnitt ausgefuehrt, um die korrekte Richtung und Gewichtung der Knoten zu ermitteln.

                            if (reader.Name.Equals("tag"))
                            {
                                //Log($"{wegid} tag erreicht");
                                if (!reader.MoveToAttribute("k"))
                                {
                                    Log($"Datenfehler: Tag {reader.Value} ohne key an Weg {wegid} gefunden.");
                                    continue;
                                }
                                /********************************************************************************
                                 ********************** READER STEHT NUN AUF DEM TAG k = ?  *********************
                                 ********************************************************************************/

                                //Log($"{wegid} {reader.Value}");
                                if (reader.Value.Equals("oneway") && reader.MoveToAttribute("v"))
                                {
                                    if (reader.Value.Equals("yes"))
                                    {
                                        einbahnAuto = true;
                                        isAndersherumAuto = false;
                                        autoRueckw = false;
                                        if (!explizitNichtEinbahnRad)
                                        {
                                            einbahnRad = true;
                                            isAndersherumRad = false;
                                            radRueckw = false;
                                        }
                                    }
                                    else if (reader.Value.Equals(-1))
                                    {
                                        einbahnAuto = true;
                                        autoVorw = false;
                                        if (!explizitNichtEinbahnRad)
                                        {
                                            einbahnRad = true;
                                            radVorw = false;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("oneway:bicycle") && reader.MoveToAttribute("v"))
                                {
                                     if (!reader.Value.Equals("yes"))
                                     {
                                         einbahnAuto = true;
                                         einbahnRad = false;
                                         isAndersherumAuto = false;
                                         explizitNichtEinbahnRad = true;
                                         autoRueckw = false;
                                     }
                                }
                                //Seitenstreifen dürfen nach StVO explizit von Radfahrern genutzt werden, insofern keine anderweitige Indikation dagegenspricht.
                                else if (reader.Value.Equals("shoulder") && reader.MoveToAttribute("v"))
                                {
                                    if (reader.Value.Equals("right") || reader.Value.Equals("both"))
                                    {
                                        radVorw = true;
                                    }
                                    if (reader.Value.Equals("left") || reader.Value.Equals("both"))
                                    {
                                        radRueckw = true;
                                    }
                                }
                                else if (reader.Value.Equals("shoulder:right") && reader.MoveToAttribute("v"))
                                {
                                    if (reader.Value.Equals("yes"))
                                    {
                                        radVorw = true;
                                    }
                                }
                                else if (reader.Value.Equals("shoulder:left") && reader.MoveToAttribute("v"))
                                {
                                    if (reader.Value.Equals("yes"))
                                    {
                                        radRueckw = true;
                                    }
                                }
                                //Radwege in Deutschland ("cycleway" sollte es in D eigentlich nicht geben?)
                                else if (reader.Value.Equals("bicycle_road") && reader.MoveToAttribute("v"))
                                {
                                    if (reader.Value.Equals("yes"))
                                    {
                                        radVorw = true;
                                        radRueckw = true;
                                        radVorwExplizitErlaubt = true;
                                        radRueckwExplizitErlaubt = true;
                                        if (!fussExplizitErlaubt)
                                        {
                                            fuss = false;
                                        }
                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                    }
                                }
                                else if ((reader.Value.Equals("cycleway") || reader.Value.StartsWith("cycleway:both") || reader.Value.Equals("cycleway:lane")) 
                                    && reader.MoveToAttribute("v"))
                                {
                                     if (reader.Value.Equals("opposite") || reader.Value.Equals("opposite_lane") ||
                                         reader.Value.Equals("track") || reader.Value.Equals("opposite_track"))
                                     {
                                         einbahnAuto = true;
                                         einbahnRad = false;
                                         isAndersherumAuto = false;
                                         explizitNichtEinbahnRad = true;
                                         autoRueckw = false;
                                     }
                                }
                                else if ((reader.Value.StartsWith("cycleway:right") || reader.Value.StartsWith("cycleway:left")) && reader.MoveToAttribute("v"))
                                {
                                    einbahnRad = false;
                                }
                                else if (reader.Value.Equals("cycleway:right:oneway") && reader.MoveToAttribute("v"))
                                {
                                    einbahnRad = true;
                                    radVorw = true;
                                    if (!radRueckwExplizitErlaubt)
                                    {
                                        radRueckw = false;
                                    }
                                }
                                else if (reader.Value.Equals("cycleway:left:oneway") && reader.MoveToAttribute("v"))
                                {
                                    einbahnRad = true;
                                    radRueckw = true;
                                    if (!radVorwExplizitErlaubt)
                                    {
                                        radVorw = false;
                                    }
                                }
                                //Umgang mit Geschwindigkeitstags. Der Eintrag "default" würde unbegrenzt bedeuten.
                                else if (reader.Value.Equals("maxspeed") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        maxspeedVorwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedVorwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                        maxspeedRueckwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedRueckwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    }
                                }
                                else if (reader.Value.Equals("maxspeed:variable") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        maxspeedVorwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedVorwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                        maxspeedRueckwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedRueckwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    }
                                }
                                else if (reader.Value.Equals("maxspeed:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        maxspeedVorwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedVorwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                    }
                                }
                                else if (reader.Value.Equals("maxspeed:variable:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        maxspeedVorwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedVorwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                    }
                                }
                                else if (reader.Value.Equals("maxspeed:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        maxspeedRueckwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedRueckwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                    }
                                }
                                else if (reader.Value.Equals("maxspeed:variable:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        maxspeedRueckwaertsAuto = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                        maxspeedRueckwaertsRad = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                        maxspeedFuss = CalculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                    }
                                }
                                //In dem Fall soll beispielsweise ein Fußgänger oder Fahrrad ausgeschlossen werden.
                                else if (reader.Value.Equals("minspeed") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        double min = getMindestgeschwindigkeit(reader);
                                        if (min > MAX_FUSS && !fussExplizitErlaubt)
                                        {
                                            fuss = false;
                                        }
                                        if (min > MAX_RAD)
                                        {
                                            if (!radVorwExplizitErlaubt)
                                            {
                                                radVorw = false;
                                            }
                                            if (!radRueckwExplizitErlaubt)
                                            {
                                                radRueckw = false;
                                            }
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("minspeed:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        double min = getMindestgeschwindigkeit(reader);
                                        if (min > MAX_FUSS && !fussExplizitErlaubt)
                                        {
                                            fuss = false;
                                        }
                                        if (min > MAX_RAD && !radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("minspeed:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                    {
                                        double min = getMindestgeschwindigkeit(reader);
                                        if (min > MAX_FUSS && !fussExplizitErlaubt)
                                        {
                                            fuss = false;
                                        }
                                        if (min > MAX_RAD && !radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("bicycle") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        radVorwExplizitErlaubt = true;
                                        radRueckwExplizitErlaubt = true;
                                        maxspeedVorwaertsRad = MAX_RAD;
                                        maxspeedRueckwaertsRad = MAX_RAD;
                                        if (reader.Value.Equals("dismount"))
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                            maxspeedRueckwaertsRad = MAX_FUSS;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("bicycle:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        radVorwExplizitErlaubt = true;
                                        maxspeedVorwaertsRad = MAX_RAD;
                                        if (reader.Value.Equals("dismount"))
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("bicycle:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no"))
                                    {
                                        radRueckwExplizitErlaubt = true;
                                        maxspeedRueckwaertsRad = MAX_RAD;
                                        if (reader.Value.Equals("dismount"))
                                        {
                                            maxspeedRueckwaertsRad = MAX_FUSS;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("vehicle") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                        autoRueckwExplizitErlaubt = true;
                                        radVorwExplizitErlaubt = true;
                                        radRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("vehicle:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                        radVorwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("vehicle:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoRueckwExplizitErlaubt = true;
                                        radRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motor_vehicle") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                        autoRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motor_vehicle:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motor_vehicle:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motorcar") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                        autoRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motorcar:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoVorwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("motorcar:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private") &&
                                            !reader.Value.Equals("agricultural") && !reader.Value.Equals("forestry"))
                                    {
                                        autoRueckwExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("footway") && reader.MoveToAttribute("v"))
                                {
                                    fussExplizitErlaubt = true;
                                    maxspeedFuss = MAX_FUSS;
                                }
                                else if (reader.Value.Equals("foot") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                    {
                                        fussExplizitErlaubt = true;
                                        maxspeedFuss = MAX_FUSS;
                                    }
                                }
                                else if (reader.Value.Equals("foot:forward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                    {
                                        fussExplizitErlaubt = true;
                                        maxspeedFuss = MAX_FUSS;
                                    }
                                }
                                else if (reader.Value.Equals("foot:backward") && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                    {
                                        fussExplizitErlaubt = true;
                                    }
                                }
                                else if((reader.Value.Equals("sidewalk") || reader.Value.Equals("sidewalk:left") || reader.Value.Equals("sidewalk:right") ||
                                    reader.Value.Equals("sidewalk:left:segregated") || reader.Value.Equals("sidewalk:right:segregated") ||
                                    reader.Value.Equals("sidewalk:both") || reader.Value.Equals("sidewalk:segregated"))
                                    && reader.MoveToAttribute("v"))
                                {
                                    if (!reader.Value.Equals("no") && !!reader.Value.Equals("none"))
                                    {
                                        fussExplizitErlaubt = true;
                                    }
                                }
                                else if (reader.Value.Equals("sidewalk:left:foot") || reader.Value.Equals("sidewalk:right:foot"))
                                {
                                    if (!reader.Value.Equals("no") && !!reader.Value.Equals("none"))
                                    {
                                        fussExplizitErlaubt = true;
                                        fuss = true;
                                    }
                                }
                                else if (reader.Value.Equals("sidewalk:left:bicycle") || reader.Value.Equals("sidewalk:right:bicycle"))
                                {
                                    if (!reader.Value.Equals("no") && !!reader.Value.Equals("none"))
                                    {
                                        radVorw = true;
                                        radRueckw = true;
                                    }
                                }
                                else if (reader.Value.Equals("sidewalk:right:oneway"))
                                {
                                    if (!reader.Value.Equals("no") && !!reader.Value.Equals("none"))
                                    {
                                        if (!reader.Value.Equals("-1"))
                                        {
                                            radVorw = true;
                                            radRueckw = false;
                                            isAndersherumRad = false;
                                        }
                                        else
                                        {
                                            radVorw = false;
                                            radRueckw = true;
                                            isAndersherumRad = true;
                                        }
                                        einbahnRad = true;
                                    }
                                }
                                //Ist das so korrekt oder wird Sidewalk Left per default als in Gegenrichtung angenommen?
                                else if (reader.Value.Equals("sidewalk:left:oneway"))
                                {
                                    if (!reader.Value.Equals("no") && !!reader.Value.Equals("none"))
                                    {
                                        if (!reader.Value.Equals("-1"))
                                        {
                                            radVorw = true;
                                            radRueckw = false;
                                            isAndersherumRad = false;
                                        }
                                        else
                                        {
                                            radVorw = false;
                                            radRueckw = true;
                                            isAndersherumRad = true;
                                        }
                                        einbahnRad = true;
                                    }
                                }
                                else if (reader.Value.Equals("highway") && reader.MoveToAttribute("v"))
                                {
                                    istWeg = true;
                                    if (reader.Value.Equals("footway"))
                                    {
                                        maxspeedFuss = MAX_FUSS;

                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsAuto = MAX_FUSS;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_FUSS;
                                        }
                                        if (!radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                        }
                                        if (!radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsRad = MAX_FUSS;
                                        }
                                    }
                                    else if (reader.Value.Equals("pedestrian"))
                                    {
                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsAuto = MAX_FUSS;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_FUSS;
                                        }
                                        if (!radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                        }
                                        if (!radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsRad = MAX_FUSS;
                                        }
                                    }
                                    else if (reader.Value.Equals("cycleway") || reader.Value.Equals("cycleway:segregated"))
                                    {
                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsAuto = MAX_RAD;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_RAD;
                                        }
                                        if (!fussExplizitErlaubt)
                                        {
                                            fuss = false;
                                        }
                                    }
                                    else if (reader.Value.Equals("motorway"))
                                    {
                                        radVorw = false;
                                        radRueckw = false;
                                        fuss = false;
                                    }
                                    else if(reader.Value.Equals("access") && reader.MoveToAttribute("v"))
                                    {
                                        if(reader.Value.Equals("no") || reader.Value.Equals("private") || reader.Value.Equals("delivery") || 
                                            reader.Value.Equals("agricultural") || reader.Value.Equals("forestry"))
                                        {
                                            if (!autoVorwExplizitErlaubt)
                                            {
                                                autoVorw = false;
                                            }
                                            if (!autoRueckwExplizitErlaubt)
                                            {
                                                autoRueckw = false;
                                            }
                                            if (!radVorwExplizitErlaubt)
                                            {
                                                radVorw = false;
                                            }
                                            if (!radRueckwExplizitErlaubt)
                                            {
                                                radRueckw = false;
                                            }
                                            if (!fussExplizitErlaubt)
                                            {
                                                fuss = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string s = reader.Name + " " + reader.Value;
                                        if (!unverarbeiteteTags.Contains(s))
                                        {
                                            unverarbeiteteTags.Add(s);
                                            Log("Nicht verarbeiteter Highwaytag: " + s);
                                        }
                                    }
                                }
                                else
                                {
                                    string s = reader.Name + " " + reader.Value;
                                    if (!unverarbeiteteTags.Contains(s) && !IrrelevanteTags.Contains(s))
                                    {
                                        unverarbeiteteTags.Add(s);
                                        Log("Nicht verarbeiteter Tag: " + s);
                                    }
                                }
                            }
                        }//Ende while (reader.Read() && reader.IsStartElement() && reader.Depth == suchtiefeKnoten)

                        /*
                         * WURDE BIS HIERHER KEINE GESCHWINDIGKEIT ERMITTELT, MUSS DIESE AUS DER LAGE (INNERORTS, AUSSERORTS) ODER DEM STRASSENTYP HERVORGEHEN.
                         */


                                //Nur wenn es sich bei Way um einen Highway handelt, ist es eine Straße, ein Pfad, Weg o.ä. im engeren Sinne.
                                if (istWeg)
                        {
                            SchreibeWeg(reader, ziel, wegid, weg, einbahnAuto, einbahnRad, isAndersherumAuto, isAndersherumRad, maxspeedVorwaertsAuto,
                            maxspeedRueckwaertsAuto, maxspeedVorwaertsRad, maxspeedRueckwaertsRad, maxspeedFuss,
                            autoVorw, autoRueckw, radVorw, radRueckw, fuss);
                        }
                    }// Ende if reader.Name.Equals("way")

                    //Ist der Knoten weder Weg noch Kartenknoten, so können er selbst und all seine Untereintraege uebersprungen werden.
                    //else
                    //{
                    //    reader.ReadToNextSibling();
                    //}
                }
                Console.WriteLine(anzahlKnoten+" Knoten geschrieben.");
                Console.ReadLine();
            }
        }

        private static void Log(string text)
        {
            byte[] b = new UTF8Encoding(true).GetBytes(text+"\n");
            log.Write(b, 0, b.Length);
            log.Flush();
        }

        private static void SchreibeWeg(XmlReader reader, FileStream ziel, string wegid, List<string> weg, bool einbahnAuto, bool einbahnRad, bool isAndersherumAuto, 
            bool isAndersherumRad, double maxspeedVorwAuto, double maxspeedRueckwAuto, double maxspeedVorwRad, double maxspeedRueckwRad, double maxspeedFuss, 
            bool autoVorw, bool autoRueckw, bool radVorw, bool radRueckw, bool fuss)
        {
            //Annahme: Liste Weg ist in der richtigen Reihenfolge!! Die Knoten werden immer in der richtigen Reihenfolge abgespeichert samt der Information,
            //ob der Weg umgekehrt passierbar ist.
            if (einbahnAuto && einbahnRad)
            {
                if (isAndersherumAuto && isAndersherumRad)
                {
                    weg.Reverse();
                }
            }

            string toWrite = "w:\t\n" + wegid + "\n\t{\n";
            if (weg.Count == 0)
            {
                Log($"Datenfehler: Der Weg {wegid} enthaelt keine Knoten.");
            }
            if(maxspeedVorwAuto > MAX_AUTO || maxspeedRueckwAuto > MAX_AUTO)
            {
                Log($"Datenfehler: Der Weg {wegid} hat eine zu hohe Geschwindigkeit für Autos.");
            }
            if (maxspeedVorwRad > MAX_RAD || maxspeedRueckwRad > MAX_RAD)
            {
                Log($"Datenfehler: Der Weg {wegid} hat eine zu hohe Geschwindigkeit für Fahrräder.");
            }
            if (maxspeedFuss > MAX_FUSS)
            {
                Log($"Datenfehler: Der Weg {wegid} hat eine zu hohe Geschwindigkeit für Fußgänger.");
            }
            if (maxspeedVorwAuto < 0 || maxspeedRueckwAuto < 0 || maxspeedVorwRad < 0 || maxspeedRueckwRad < 0 || maxspeedFuss < 0)
            {
                Log($"Datenfehler: Der Weg {wegid} hat eine negative Geschwindigkeit");
            }
            foreach (string knotenid in weg)
            {
                toWrite += "\t"+knotenid+"\n";
            }
            toWrite += "\t}\n " +
                "\ta:" + maxspeedVorwAuto + ";" + maxspeedRueckwAuto + ";" + einbahnAuto + ";" + autoVorw + ";" + autoRueckw + "\n" +
                "\tr:" + maxspeedVorwRad + ";" + maxspeedRueckwRad + ";" + einbahnRad + ";" + radVorw + ";" + radRueckw + "\n" +
                "\tf:" + maxspeedFuss + ";" + fuss + "\n";

            byte[] b = new UTF8Encoding(true).GetBytes(toWrite);
            ziel.Write(b, 0, b.Length);
            //ziel.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
            ziel.Flush();
        }

        private static double getMindestgeschwindigkeit(XmlReader reader)
        {
            double min = 0;
            if (reader.Value.Equals("DE:Autobahn"))
            {
                min = 60;
            }
            else
            {
                min = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            }

            return min;
        }

        //Berechnet das vorlaeufige Kantengewicht als Kehrwert der aktuell minimal bekannten Maximalgeschwindigkeit.
        private static double CalculateVorlaeufigeMaxspeed(XmlReader reader, string wegid, double aktMax)
        {
            double max = MAX_AUTO;
            switch (reader.Value)
            {
                case "DE:Landstraße":
                case "DE:rural":
                    max = 100;
                    break;
                case "NL:Landstraße":
                case "NL:rural":
                    max = 80;
                    break;
                case "DE:Autobahn":
                case "DE:motorway":
                    max = 100;
                    break;
                case "DE:bicycle_road":
                    max = 30;
                    break;
                case "DE:Innerorts":
                case "NL:Innerorts":
                case "DE:urban":
                case "NL:urban":
                    max = 50;
                    break;
                case "walk":
                case "DE:living_street":
                    max = 15;
                    break;
                //Tritt auf, wenn maxspeed:variable o.ä. Dann muss das Gewicht anderweitig definiert werden, tue also nichts.
                case "yes":
                    //Log($"Case yes bei Wegid {wegid}");
                    break;
                default:
                    max = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                    //Log($"Case default es bei Wegid {wegid}. Errechneter Wert {max}");
                    break;
            }
            return Math.Min(max, aktMax);
        }
    }
}
