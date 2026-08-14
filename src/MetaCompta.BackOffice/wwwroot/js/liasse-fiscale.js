/**
 * MetaCompta Liasse Fiscale v4.0 — Excel-like Accounting Workspace
 * Every cell: shows value + formula reference from Balance
 * Click any cell: configure formula (change which accounts are used)
 * Per-client formula storage • Standard CGNC Moroccan structure
 */
var LiasseFiscale = (function(){
'use strict';

/* ═══ 1. PCM — Plan Comptable Marocain Normalisé ═══ */
var PCM = (function(){
    var _a = {
        '1111':{n:'Capital social',c:1},'1140':{n:'Réserve légale',c:1},'1152':{n:'Réserves facultatives',c:1},
        '1181':{n:'Report à nouveau (SC)',c:1},'1191':{n:'Résultat net (bénéfice)',c:1},'1199':{n:'Résultat net (perte)',c:1},
        '1481':{n:'Emprunts ét. crédit',c:1},'1486':{n:'Fournisseurs immob.',c:1},
        '1511':{n:'Provisions litiges',c:1},'1555':{n:'Provisions charges à répartir',c:1},
        '2111':{n:'Frais de constitution',c:2},'2230':{n:'Fonds commercial',c:2},
        '2311':{n:'Terrains nus',c:2},'2321':{n:'Bâtiments',c:2},'2332':{n:'Mat. outillage',c:2},
        '2340':{n:'Mat. transport',c:2},'2351':{n:'Mobilier bureau',c:2},'2352':{n:'Mat. informatique',c:2},
        '2355':{n:'Mat. bureau',c:2},'2393':{n:'Immob. corp. en cours',c:2},
        '2411':{n:'Prêts au personnel',c:2},'2486':{n:'Dépôts cautionnements',c:2},'2510':{n:'Titres participation',c:2},
        '2811':{n:'Amort. frais constitution',c:2},'2832':{n:'Amort. mat. outillage',c:2},
        '2834':{n:'Amort. mat. transport',c:2},'2835':{n:'Amort. mob. bureau info',c:2},
        '2920':{n:'Prov. dépr. immob. corp.',c:2},
        '3111':{n:'Marchandises',c:3},'3121':{n:'Matières premières',c:3},'3151':{n:'Produits finis',c:3},
        '3421':{n:'Clients',c:3},'3424':{n:'Clients douteux',c:3},'3427':{n:'Clients FAE',c:3},
        '3431':{n:'Avances personnel',c:3},'34521':{n:'TVA récupérable',c:3},'3453':{n:'Acomptes IS',c:3},
        '3455':{n:'TVA à récupérer',c:3},'3458':{n:'État débiteurs',c:3},'3488':{n:'Divers débiteurs',c:3},
        '3491':{n:'Prov. dépr. clients',c:3},'3500':{n:'TVP',c:3},
        '4411':{n:'Fournisseurs',c:4},'4415':{n:'Fourn. effets payer',c:4},'4417':{n:'Fourn. FNP',c:4},
        '4432':{n:'Rémunérations dues',c:4},'4437':{n:'Charges pers. payer',c:4},
        '4441':{n:'CNSS',c:4},'4443':{n:'Caisses retraite',c:4},'4448':{n:'Autres org. sociaux',c:4},
        '44521':{n:'TVA facturée',c:4},'4453':{n:'IS',c:4},'4456':{n:'TVA due',c:4},'4458':{n:'État créditeurs',c:4},
        '4481':{n:'Dettes acquis. immob.',c:4},'4488':{n:'Divers créditeurs',c:4},
        '5141':{n:'Banques (SD)',c:5},'5143':{n:'Trésorerie Générale',c:5},'5161':{n:'Caisse',c:5},
        '5541':{n:'Banques (SC)',c:5},'5520':{n:'Crédits trésorerie',c:5},'5115':{n:'Chèques encaisser',c:5},
        '6111':{n:'Achats marchandises',c:6},'6114':{n:'Var. stocks march.',c:6},
        '6121':{n:'Achats mat. premières',c:6},'6124':{n:'Var. stocks MP',c:6},
        '6125':{n:'Achats non stockés',c:6},'6126':{n:'Achats travaux',c:6},
        '6131':{n:'Locations',c:6},'6132':{n:'Crédit-bail',c:6},'6133':{n:'Entretien répar.',c:6},
        '6134':{n:'Assurances',c:6},'6141':{n:'Études recherches',c:6},'6142':{n:'Transports',c:6},
        '6143':{n:'Déplacements',c:6},'6144':{n:'Publicité',c:6},'6145':{n:'Frais postaux',c:6},
        '6147':{n:'Services bancaires',c:6},'6149':{n:'Autres services ext.',c:6},
        '6161':{n:'Impôts taxes directs',c:6},'6165':{n:'Impôts taxes indirects',c:6},
        '6167':{n:'Droits enregistr.',c:6},
        '6171':{n:'Rémunérations',c:6},'6174':{n:'Charges sociales',c:6},'6176':{n:'Ch. sociales div.',c:6},
        '6191':{n:'DEA non-valeurs',c:6},'6193':{n:'DEA immob. corp.',c:6},
        '6196':{n:'Prov. dépr. actif',c:6},'6393':{n:'Dot. prov. stocks',c:6},'6396':{n:'Dot. prov. clients',c:6},
        '6311':{n:'Intérêts emprunts',c:6},'6382':{n:'Pertes change',c:6},'6386':{n:'Escomptes accordés',c:6},
        '6513':{n:'VNA immob. cédées',c:6},'6581':{n:'Pénalités amendes',c:6},
        '6701':{n:'IS',c:6},
        '7111':{n:'Ventes marchandises',c:7},'7121':{n:'Ventes biens produits',c:7},
        '7124':{n:'Var. stocks produits',c:7},'7126':{n:'Immob. produites',c:7},
        '7128':{n:'Prestations services',c:7},'7131':{n:'Subventions expl.',c:7},
        '7148':{n:'Autres produits expl.',c:7},
        '7181':{n:'Reprises prov. amort.',c:7},'7186':{n:'Reprises prov. circ.',c:7},
        '7311':{n:'Intérêts produits',c:7},'7321':{n:'Revenus titres part.',c:7},
        '7382':{n:'Gains change',c:7},'7386':{n:'Escomptes obtenus',c:7},
        '7513':{n:'PC immob. cédées',c:7},'7581':{n:'Pénalités reçues',c:7}
    };
    return {
        get:function(c){return _a[c]||null;},
        name:function(c){var a=_a[c];return a?c+' '+a.n:c;},
        classOf:function(c){return parseInt((c+'')[0])||0;},
        all:function(){return Object.assign({},_a);},
        byClass:function(cls){var r={};for(var k in _a)if(_a[k].c===cls)r[k]=_a[k];return r;}
    };
})();

/* ═══ 2. STORAGE — Per client + exercice ═══ */
var Storage = (function(){
    var _pfx='lf_', _cpfx='lfc_';
    return {
        setContext:function(declId,clientId,exercice){
            var ex = exercice || '0';
            var cid = clientId || '0';
            _pfx = 'lf_'+cid+'_'+ex+'_';
            _cpfx = 'lfc_'+cid+'_'+ex+'_';
            if(declId){ /* legacy key still readable for overrides if needed */ }
        },
        get:function(k){try{return JSON.parse(localStorage.getItem(_pfx+k));}catch(e){return null;}},
        set:function(k,v){try{localStorage.setItem(_pfx+k,JSON.stringify(v));}catch(e){}},
        remove:function(k){localStorage.removeItem(_pfx+k);},
        getFormula:function(cellCode){var f=this.getClientFormulas();return f[cellCode]||null;},
        setFormula:function(cellCode,formula){var f=this.getClientFormulas();f[cellCode]=formula;try{localStorage.setItem(_cpfx+'formulas',JSON.stringify(f));}catch(e){}},
        removeFormula:function(cellCode){var f=this.getClientFormulas();delete f[cellCode];try{localStorage.setItem(_cpfx+'formulas',JSON.stringify(f));}catch(e){}},
        getClientFormulas:function(){try{return JSON.parse(localStorage.getItem(_cpfx+'formulas'))||{};}catch(e){return {};}},
        getOverride:function(c){var o=this.get('overrides')||{};return o[c]!==undefined?o[c]:null;},
        setOverride:function(c,v){var o=this.get('overrides')||{};o[c]=v;this.set('overrides',o);},
        removeOverride:function(c){var o=this.get('overrides')||{};delete o[c];this.set('overrides',o);},
        clearOverrides:function(){this.remove('overrides');}
    };
})();

/* ═══ 3. JOURNAL ENGINE ═══ */
var Journal = (function(){
    var _entries = [];
    function _demo(yr){
        var e=[], m=function(mo,dy,pc,lb,lines){
            var d=yr+'-'+(mo<10?'0':'')+mo+'-'+(dy<10?'0':'')+dy;
            lines.forEach(function(l){e.push({date:d,piece:pc,account:l[0],label:lb,debit:l[1]||0,credit:l[2]||0,journal:l[3]||'OD'});});
        };
        m(1,2,'AC01','Capital initial',[['5141',500000,0,'BQ'],['1111',0,500000,'BQ']]);
        m(1,5,'AC02','Acq. mobilier',[['2351',85000,0,'AC'],['4411',0,85000,'AC']]);
        m(1,5,'AC03','Acq. mat. info',[['2352',120000,0,'AC'],['4411',0,120000,'AC']]);
        m(1,10,'FA01','Achats march. janv.',[['6111',180000,0,'HA'],['34521',36000,0,'HA'],['4411',0,216000,'HA']]);
        m(1,15,'VT01','Ventes janv.',[['3421',360000,0,'VT'],['7111',0,300000,'VT'],['44521',0,60000,'VT']]);
        m(1,25,'BQ01','Loyer janv.',[['6131',25000,0,'BQ'],['5141',0,25000,'BQ']]);
        m(1,31,'SA01','Salaires janv.',[['6171',95000,0,'OD'],['6174',28500,0,'OD'],['4432',0,85000,'OD'],['4441',0,23750,'OD'],['4443',0,9500,'OD'],['44521',0,5250,'OD']]);
        m(2,3,'BQ02','Enc. client VT01',[['5141',360000,0,'BQ'],['3421',0,360000,'BQ']]);
        m(2,10,'FA02','Achats fév.',[['6111',165000,0,'HA'],['34521',33000,0,'HA'],['4411',0,198000,'HA']]);
        m(2,15,'VT02','Ventes fév.',[['3421',480000,0,'VT'],['7111',0,400000,'VT'],['44521',0,80000,'VT']]);
        m(2,20,'BQ03','Paiement fourn.',[['4411',216000,0,'BQ'],['5141',0,216000,'BQ']]);
        m(2,25,'BQ04','Loyer fév.',[['6131',25000,0,'BQ'],['5141',0,25000,'BQ']]);
        m(2,28,'SA02','Salaires fév.',[['6171',97000,0,'OD'],['6174',29100,0,'OD'],['4432',0,87000,'OD'],['4441',0,24300,'OD'],['4443',0,9700,'OD'],['44521',0,5100,'OD']]);
        m(3,10,'FA03','Fourn. diverses',[['6125',35000,0,'HA'],['34521',7000,0,'HA'],['4411',0,42000,'HA']]);
        m(3,15,'VT03','Ventes mars',[['3421',540000,0,'VT'],['7111',0,450000,'VT'],['44521',0,90000,'VT']]);
        m(3,20,'BQ05','Assurance',[['6134',18000,0,'BQ'],['5141',0,18000,'BQ']]);
        m(3,25,'BQ06','Télécom T1',[['6145',8500,0,'BQ'],['34521',1700,0,'BQ'],['5141',0,10200,'BQ']]);
        m(3,31,'IS01','Acompte IS T1',[['3453',45000,0,'BQ'],['5141',0,45000,'BQ']]);
        m(3,31,'SA03','Salaires mars',[['6171',97000,0,'OD'],['6174',29100,0,'OD'],['4432',0,87000,'OD'],['4441',0,24300,'OD'],['4443',0,9700,'OD'],['44521',0,5100,'OD']]);
        m(3,31,'TVA1','TVA janv.',[['44521',60000,0,'OD'],['34521',36000,0,'OD'],['4456',0,24000,'OD']]);
        m(4,10,'FA04','Achats T2',[['6111',520000,0,'HA'],['34521',104000,0,'HA'],['4411',0,624000,'HA']]);
        m(4,15,'VT04','Ventes T2',[['3421',1800000,0,'VT'],['7111',0,1500000,'VT'],['44521',0,300000,'VT']]);
        m(5,15,'BQ07','Publicité',[['6144',45000,0,'BQ'],['34521',9000,0,'BQ'],['5141',0,54000,'BQ']]);
        m(5,31,'SA04','Salaires avr-mai',[['6171',194000,0,'OD'],['6174',58200,0,'OD'],['4432',0,174000,'OD'],['4441',0,48600,'OD'],['4443',0,19400,'OD'],['44521',0,10200,'OD']]);
        m(6,15,'BQ08','Entretien',[['6133',22000,0,'BQ'],['34521',4400,0,'BQ'],['5141',0,26400,'BQ']]);
        m(6,30,'IS02','Acompte IS T2',[['3453',45000,0,'BQ'],['5141',0,45000,'BQ']]);
        m(6,30,'SA05','Salaires juin',[['6171',98000,0,'OD'],['6174',29400,0,'OD'],['4432',0,88000,'OD'],['4441',0,24500,'OD'],['4443',0,9800,'OD'],['44521',0,5100,'OD']]);
        m(7,10,'FA05','Achats T3',[['6111',480000,0,'HA'],['34521',96000,0,'HA'],['4411',0,576000,'HA']]);
        m(7,15,'VT05','Ventes T3',[['3421',1620000,0,'VT'],['7111',0,1350000,'VT'],['44521',0,270000,'VT']]);
        m(8,10,'AC04','Acq. mat. transport',[['2340',250000,0,'AC'],['4411',0,250000,'AC']]);
        m(8,20,'BQ09','Transports dépl.',[['6142',15000,0,'BQ'],['6143',12000,0,'BQ'],['5141',0,27000,'BQ']]);
        m(9,15,'BQ10','Services banc.',[['6147',8200,0,'BQ'],['5141',0,8200,'BQ']]);
        m(9,30,'IS03','Acompte IS T3',[['3453',45000,0,'BQ'],['5141',0,45000,'BQ']]);
        m(9,30,'SA06','Salaires T3',[['6171',300000,0,'OD'],['6174',90000,0,'OD'],['4432',0,270000,'OD'],['4441',0,75000,'OD'],['4443',0,30000,'OD'],['44521',0,15000,'OD']]);
        m(10,10,'FA06','Achats T4',[['6111',550000,0,'HA'],['34521',110000,0,'HA'],['4411',0,660000,'HA']]);
        m(10,15,'VT06','Ventes T4',[['3421',2100000,0,'VT'],['7111',0,1750000,'VT'],['44521',0,350000,'VT']]);
        m(11,5,'BQ11','Paiement fourn.',[['4411',1250000,0,'BQ'],['5141',0,1250000,'BQ']]);
        m(11,20,'BQ12','Impôts taxes',[['6161',35000,0,'BQ'],['5141',0,35000,'BQ']]);
        m(11,30,'SA07','Salaires oct-nov',[['6171',200000,0,'OD'],['6174',60000,0,'OD'],['4432',0,180000,'OD'],['4441',0,50000,'OD'],['4443',0,20000,'OD'],['44521',0,10000,'OD']]);
        m(12,10,'VT07','Ventes déc.',[['3421',660000,0,'VT'],['7111',0,550000,'VT'],['44521',0,110000,'VT']]);
        m(12,15,'BQ13','Honoraires EC',[['6126',65000,0,'BQ'],['34521',13000,0,'BQ'],['5141',0,78000,'BQ']]);
        m(12,20,'IS04','Acompte IS T4',[['3453',45000,0,'BQ'],['5141',0,45000,'BQ']]);
        m(12,28,'BQ14','Enc. clients T4',[['5141',2200000,0,'BQ'],['3421',0,2200000,'BQ']]);
        m(12,31,'SA08','Salaires déc.',[['6171',102000,0,'OD'],['6174',30600,0,'OD'],['4432',0,92000,'OD'],['4441',0,25500,'OD'],['4443',0,10200,'OD'],['44521',0,4900,'OD']]);
        m(12,31,'IN01','Amort. frais const.',[['6191',2100,0,'OD'],['2811',0,2100,'OD']]);
        m(12,31,'IN02','Amort. mat. outillage',[['6193',17000,0,'OD'],['2832',0,17000,'OD']]);
        m(12,31,'IN03','Amort. mat. transport',[['6193',50000,0,'OD'],['2834',0,50000,'OD']]);
        m(12,31,'IN04','Amort. mob. info',[['6193',41000,0,'OD'],['2835',0,41000,'OD']]);
        m(12,31,'IN05','Prov. clients douteux',[['6396',25000,0,'OD'],['3491',0,25000,'OD']]);
        m(12,31,'IN06','Prov. litiges',[['6196',30000,0,'OD'],['1511',0,30000,'OD']]);
        m(12,31,'IN07','Stock final march.',[['3111',120000,0,'OD'],['6114',0,120000,'OD']]);
        m(12,31,'IN08','CCA assurance',[['3488',9000,0,'OD'],['6134',0,9000,'OD']]);
        m(12,31,'IN09','FNP déc.',[['6125',12000,0,'OD'],['4417',0,12000,'OD']]);
        m(12,31,'IN10','IS exercice',[['6701',185000,0,'OD'],['4453',0,185000,'OD']]);
        return e;
    }
    return {
        init:function(yr){_entries=_demo(yr);},
        clear:function(){_entries=[];},
        getAll:function(){return _entries.slice();},
        getByJournal:function(j){return _entries.filter(function(e){return e.journal===j;});},
        getByAccount:function(a){return _entries.filter(function(e){return e.account===a;});},
        getJournalCodes:function(){var c={};_entries.forEach(function(e){c[e.journal]=true;});return Object.keys(c).sort();},
        totalDebit:function(l){return(l||_entries).reduce(function(s,e){return s+e.debit;},0);},
        totalCredit:function(l){return(l||_entries).reduce(function(s,e){return s+e.credit;},0);}
    };
})();
/* ═══ 4. LEDGER ENGINE ═══ */
var Ledger = (function(){
    var _accts = {};
    return {
        build:function(){
            _accts={};
            Journal.getAll().forEach(function(e){
                if(!_accts[e.account])_accts[e.account]={entries:[],totalD:0,totalC:0};
                _accts[e.account].entries.push(e);
                _accts[e.account].totalD+=e.debit;
                _accts[e.account].totalC+=e.credit;
            });
        },
        loadFromLines:function(lines){
            _accts={};
            (lines||[]).forEach(function(l){
                var a = l.account || l.numeroCompte || l.NumeroCompte;
                if(!a) return;
                var d = Number(l.debit != null ? l.debit : l.totalDebit) || 0;
                var c = Number(l.credit != null ? l.credit : l.totalCredit) || 0;
                _accts[a] = {entries:[], totalD:d, totalC:c};
            });
        },
        getAccounts:function(){return Object.keys(_accts).sort();},
        getAccount:function(a){return _accts[a]||{entries:[],totalD:0,totalC:0};},
        solde:function(a){var ac=_accts[a];if(!ac)return 0;return ac.totalD-ac.totalC;},
        soldeSD:function(a){var s=this.solde(a);return s>0?s:0;},
        soldeSC:function(a){var s=this.solde(a);return s<0?-s:0;}
    };
})();

/* ═══ 5. BALANCE ENGINE ═══ */
var Balance = (function(){
    var _lines = [];
    var _fromApi = false;
    return {
        build:function(){
            _fromApi = false;
            _lines=[];
            Ledger.getAccounts().forEach(function(a){
                var ac=Ledger.getAccount(a);
                var sd=ac.totalD-ac.totalC;
                _lines.push({
                    account:a,
                    name:PCM.name(a),
                    debit:ac.totalD,
                    credit:ac.totalC,
                    soldeD:sd>0?sd:0,
                    soldeC:sd<0?-sd:0
                });
            });
        },
        loadFromApi:function(rows){
            _fromApi = true;
            _lines = (rows||[]).map(function(r){
                var acct = r.account || r.numeroCompte || r.NumeroCompte || '';
                var debit = Number(r.debit != null ? r.debit : r.totalDebit) || 0;
                var credit = Number(r.credit != null ? r.credit : r.totalCredit) || 0;
                var sd = r.soldeD != null ? Number(r.soldeD) : (r.soldeDebiteur != null ? Number(r.soldeDebiteur) : Math.max(0, debit-credit));
                var sc = r.soldeC != null ? Number(r.soldeC) : (r.soldeCrediteur != null ? Number(r.soldeCrediteur) : Math.max(0, credit-debit));
                return {
                    account: acct,
                    name: r.name || r.libelleCompte || PCM.name(acct),
                    debit: debit,
                    credit: credit,
                    soldeD: sd,
                    soldeC: sc
                };
            }).filter(function(l){return !!l.account;});
            Ledger.loadFromLines(_lines);
        },
        isFromApi:function(){return _fromApi;},
        getAll:function(){return _lines.slice();},
        byPrefix:function(pfx){return _lines.filter(function(l){return l.account.indexOf(pfx)===0;});},
        totalDebit:function(){return _lines.reduce(function(s,l){return s+l.debit;},0);},
        totalCredit:function(){return _lines.reduce(function(s,l){return s+l.credit;},0);},
        totalSD:function(){return _lines.reduce(function(s,l){return s+l.soldeD;},0);},
        totalSC:function(){return _lines.reduce(function(s,l){return s+l.soldeC;},0);}
    };
})();

/* ═══ 6. FORMULA ENGINE — CGNC formulas with configurable references ═══ */
/*
 * Each formula: {type:'SUM_SD'|'SUM_SC'|'SUM'|'SOLDE'|'REF'|'DIFF', accounts:['2111','2230',...], refs:['CELL1','CELL2']}
 * type: SUM_SD = sum of soldes débiteurs, SUM_SC = sum of soldes créditeurs,
 *       SUM = sum of (debit-credit), SOLDE = same as SUM but absolute
 *       REF = reference to other cells, DIFF = difference of two refs
 * Formula engine stores default formulas; client-specific overrides are in Storage.
 */
var Formula = (function(){
    var _defaults = {};
    var _cache = {};

    function _register(code, type, accounts, refs, label){
        _defaults[code] = {type:type, accounts:accounts||[], refs:refs||[], label:label||code};
    }

    function getFormula(code){
        /* Client override first */
        var custom = Storage.getFormula(code);
        if(custom) return custom;
        return _defaults[code] || null;
    }

    function compute(code){
        if(_cache[code]!==undefined) return _cache[code];
        var f = getFormula(code);
        if(!f) return 0;
        var val = 0;
        switch(f.type){
            case 'SUM_SD':
                f.accounts.forEach(function(a){val+=Ledger.soldeSD(a);});
                break;
            case 'SUM_SC':
                f.accounts.forEach(function(a){val+=Ledger.soldeSC(a);});
                break;
            case 'SUM':
                f.accounts.forEach(function(a){val+=Ledger.solde(a);});
                break;
            case 'SOLDE':
                f.accounts.forEach(function(a){val+=Ledger.solde(a);});
                val=Math.abs(val);
                break;
            case 'REF':
                (f.refs||[]).forEach(function(r){val+=compute(r);});
                break;
            case 'DIFF':
                if(f.refs&&f.refs.length>=1){
                    val=compute(f.refs[0]);
                    for(var di=1;di<f.refs.length;di++) val-=compute(f.refs[di]);
                }
                break;
            case 'NEG_SUM':
                f.accounts.forEach(function(a){val-=Ledger.solde(a);});
                break;
            default:
                val=0;
        }
        /* Apply manual override if exists */
        var ov=Storage.getOverride(code);
        if(ov!==null) val=ov;
        _cache[code]=val;
        return val;
    }

    /* Build the display reference string for a cell: "= Σ(2321,2340,2351)" */
    function refString(code){
        var f=getFormula(code);
        if(!f)return '';
        if(f.type==='REF') return '= '+ (f.refs||[]).join(' + ');
        if(f.type==='DIFF'){
            var refs=f.refs||[];
            if(!refs.length) return '';
            return '= '+refs[0]+(refs.length>1 ? ' − '+refs.slice(1).join(' − ') : '');
        }
        if(!f.accounts||!f.accounts.length)return '';
        var prefix = f.type==='SUM_SC' ? '= ΣSC(' : (f.type==='SUM_SD' ? '= ΣSD(' : '= Σ(');
        return prefix+f.accounts.join(',')+')';
    }

    /** Parse Excel-like formula text into a formula object (or override number). */
    function parseFormulaText(text, fallbackType){
        if(text==null) return null;
        var t = String(text).trim();
        if(!t) return { kind:'clear' };
        if(t.charAt(0)==='=') t = t.substring(1).trim();
        if(/^-?\d+([.,]\d+)?$/.test(t)){
            return { kind:'override', value: parseFloat(t.replace(',','.')) };
        }
        /* =CODE1 + CODE2 − CODE3 */
        if(/[A-Z]{2,}_[A-Z0-9]+/.test(t) && (t.indexOf('+')>=0 || t.indexOf('-')>=0 || t.indexOf('−')>=0)){
            var parts = t.replace(/−/g,'-').split(/(\+|\-)/).map(function(x){return x.trim();}).filter(Boolean);
            var refs = [];
            var op = '+';
            var first = true;
            var isDiff = false;
            parts.forEach(function(p){
                if(p==='+'||p==='-'){ op=p; return; }
                if(first){ refs.push(p); first=false; return; }
                if(op==='-'){ isDiff=true; refs.push(p); }
                else refs.push(p);
            });
            if(refs.length>=2 && isDiff && parts.filter(function(p){return p==='+';}).length===0)
                return { kind:'formula', formula:{ type:'DIFF', accounts:[], refs:refs, label:refs.join('-') } };
            if(refs.length>=1)
                return { kind:'formula', formula:{ type: isDiff ? 'DIFF' : 'REF', accounts:[], refs:refs, label:refs.join('+') } };
        }
        /* =Σ(a,b,c) or ΣSD(...) / ΣSC(...) or bare account list */
        var m = t.match(/^(?:Σ|SUM)?(?:SD|SC)?\s*\((.+)\)\s*$/i) || t.match(/^(?:ΣSD|ΣSC|SUM_SD|SUM_SC)\s*\((.+)\)\s*$/i);
        var accts = null;
        var typ = fallbackType || 'SUM_SD';
        if(m){
            accts = m[1].split(/[,;\s]+/).map(function(x){return x.trim();}).filter(Boolean);
            if(/SC/i.test(t)) typ='SUM_SC';
            else if(/SD/i.test(t)) typ='SUM_SD';
        } else if(/^\d{3,}/.test(t) || t.indexOf(',')>=0){
            accts = t.split(/[,;\s]+/).map(function(x){return x.trim();}).filter(function(x){return /^\d/.test(x);});
        }
        if(accts && accts.length){
            return { kind:'formula', formula:{ type:typ, accounts:accts, refs:[], label:'' } };
        }
        /* single cell ref */
        if(/^[A-Z]{2,}_[A-Z0-9]+$/.test(t)){
            return { kind:'formula', formula:{ type:'REF', accounts:[], refs:[t], label:t } };
        }
        return null;
    }

    function clearCache(){_cache={};}

    /* ─── Register ALL default formulas ─── */
    /* BILAN ACTIF — CGNC Maroc  (Brut) */
    _register('BA_IMN','SUM_SD',['2111','2112','2113','2114','2121'],[],'Immob. en non-valeurs');
    _register('BA_IIC','SUM_SD',['2210','2220','2230','2240','2250','2285'],[],'Immob. incorporelles');
    _register('BA_TER','SUM_SD',['2311','2312','2313','2314'],[],'Terrains');
    _register('BA_CON','SUM_SD',['2321','2323','2325','2327'],[],'Constructions');
    _register('BA_ITR','SUM_SD',['2331','2332','2333'],[],'Install. tech. mat. outillage');
    _register('BA_MTR','SUM_SD',['2340'],[],'Matériel de transport');
    _register('BA_MMB','SUM_SD',['2351','2352','2355','2356'],[],'Mob. mat. bureau & info');
    _register('BA_AIC','SUM_SD',['2380','2385','2388'],[],'Autres immob. corporelles');
    _register('BA_IEC','SUM_SD',['2393','2394','2395','2397'],[],'Immob. corp. en cours');
    _register('BA_PRT','SUM_SD',['2411','2415','2416','2418'],[],'Prêts immobilisés');
    _register('BA_AIF','SUM_SD',['2481','2483','2486','2487','2488'],[],'Autres créances financières');
    _register('BA_TPR','SUM_SD',['2510','2550','2581'],[],'Titres de participation');

    /* Amortissements Actif */
    _register('BA_AMN','SUM_SC',['2811','2812','2813'],[],'Amort. non-valeurs');
    _register('BA_AII','SUM_SC',['2821','2822','2823'],[],'Amort. immob. incorp.');
    _register('BA_ATE','SUM_SC',['2831'],[],'Amort. terrains');
    _register('BA_ACO','SUM_SC',['2832'],[],'Amort. constructions');
    _register('BA_AIT','SUM_SC',['2833','2832'],[],'Amort. ITMO');
    _register('BA_AMT','SUM_SC',['2834'],[],'Amort. mat. transport');
    _register('BA_AMM','SUM_SC',['2835','2836'],[],'Amort. MMB');

    /* Totaux Actif Immobilisé */
    _register('BA_TIMM','REF',[],['BA_IMN','BA_IIC','BA_TER','BA_CON','BA_ITR','BA_MTR','BA_MMB','BA_AIC','BA_IEC','BA_PRT','BA_AIF','BA_TPR'],'Total Actif Immobilisé');
    _register('BA_TAMM','REF',[],['BA_AMN','BA_AII','BA_ATE','BA_ACO','BA_AIT','BA_AMT','BA_AMM'],'Total Amort. Immob.');

    /* Actif Circulant */
    _register('BA_STK','SUM_SD',['3111','3112','3121','3122','3151','3152'],[],'Stocks');
    _register('BA_CLI','SUM_SD',['3421','3423','3424','3425','3427'],[],'Clients & rattachés');
    _register('BA_PER','SUM_SD',['3431','3438'],[],'Personnel débiteur');
    _register('BA_ETA','SUM_SD',['3451','34521','3453','3455','3458'],[],'État débiteur');
    _register('BA_CDA','SUM_SD',['3481','3488'],[],'Comptes associés & divers débiteurs');
    _register('BA_CRA','SUM_SD',['3491','3493','3496'],[],'Provisions dépr. actif circ.');

    _register('BA_TCIR','REF',[],['BA_STK','BA_CLI','BA_PER','BA_ETA','BA_CDA'],'Total Actif Circulant');

    /* Trésorerie Actif */
    _register('BA_TVP','SUM_SD',['3500','3501','3502'],[],'TVP');
    _register('BA_BQA','SUM_SD',['5111','5115','5141','5143','5161','5165'],[],'Trésorerie - Actif');
    _register('BA_TTRA','REF',[],['BA_TVP','BA_BQA'],'Total Trésorerie Actif');

    /* TOTAL GÉNÉRAL ACTIF */
    _register('BA_TOTAL','REF',[],['BA_TIMM','BA_TCIR','BA_TTRA'],'TOTAL ACTIF');

    /* BILAN PASSIF — CGNC Maroc */
    _register('BP_CAP','SUM_SC',['1111','1112','1117','1119'],[],'Capital social');
    _register('BP_RES','SUM_SC',['1140','1151','1152','1155'],[],'Réserves');
    _register('BP_REP','SUM',['1181','1189'],[],'Report à nouveau');
    _register('BP_RNE','SUM',['1191','1199'],[],'Résultat net');
    _register('BP_TFP','REF',[],['BP_CAP','BP_RES','BP_REP','BP_RNE'],'Total Fonds Propres');

    _register('BP_SUB','SUM_SC',['1311','1312'],[],'Subventions investissement');
    _register('BP_PRV','SUM_SC',['1411','1421','1431','1481'],[],'Provisions réglementées');
    _register('BP_TCP','REF',[],['BP_TFP','BP_SUB','BP_PRV'],'Total Capitaux Propres');

    _register('BP_EMP','SUM_SC',['1481','1482','1484','1486'],[],'Dettes de financement');
    _register('BP_PDD','SUM_SC',['1511','1512','1513','1555'],[],'Provisions durables');
    _register('BP_ECT','SUM_SC',['1701','1702'],[],'Écarts de conversion');

    _register('BP_TFIN','REF',[],['BP_TCP','BP_EMP','BP_PDD','BP_ECT'],'Total Financement Permanent');

    /* Passif Circulant */
    _register('BP_FRS','SUM_SC',['4411','4413','4415','4417','4418'],[],'Fournisseurs & rattachés');
    _register('BP_PRS','SUM_SC',['4432','4434','4437','4438'],[],'Personnel créditeur');
    _register('BP_OSO','SUM_SC',['4441','4443','4445','4447','4448'],[],'Organismes sociaux');
    _register('BP_ETC','SUM_SC',['44521','4453','4455','4456','4458'],[],'État créditeur');
    _register('BP_DIV','SUM_SC',['4481','4484','4488'],[],'Comptes associés & divers créditeurs');
    _register('BP_PPC','SUM_SC',['4491','4493','4495'],[],'Prov. passif circulant');

    _register('BP_TCIR','REF',[],['BP_FRS','BP_PRS','BP_OSO','BP_ETC','BP_DIV'],'Total Passif Circulant');

    /* Trésorerie Passif */
    _register('BP_CTR','SUM_SC',['5520','5530','5541','5548'],[],'Trésorerie - Passif');
    _register('BP_TTRP','REF',[],['BP_CTR'],'Total Trésorerie Passif');

    /* TOTAL GÉNÉRAL PASSIF */
    _register('BP_TOTAL','REF',[],['BP_TFIN','BP_TCIR','BP_TTRP'],'TOTAL PASSIF');

    /* ─── CPC — Compte Produits & Charges ─── */
    /* Produits exploitation */
    _register('CPC_VMH','SUM_SC',['7111','7112','7113'],[],'Ventes march. (en l\'état)');
    _register('CPC_VBP','SUM_SC',['7121','7122','7124','7126'],[],'Ventes biens & services produits');
    _register('CPC_VST','SUM_SC',['7128','7129'],[],'Prestations services');
    _register('CPC_CAH','REF',[],['CPC_VMH'],'CA - Marchandises');
    _register('CPC_CAP','REF',[],['CPC_VBP','CPC_VST'],'CA - Production');
    _register('CPC_CA','REF',[],['CPC_CAH','CPC_CAP'],'Chiffre d\'affaires');
    _register('CPC_VSP','SUM_SC',['7131','7134'],[],'Subventions expl.');
    _register('CPC_APE','SUM_SC',['7141','7148','7149'],[],'Autres produits exploitation');
    _register('CPC_RPE','SUM_SC',['7181','7182','7185','7186','7188'],[],'Reprises, transferts ch.');
    _register('CPC_TPE','REF',[],['CPC_CA','CPC_VSP','CPC_APE','CPC_RPE'],'TOTAL PRODUITS EXPLOITATION');

    /* Charges exploitation */
    _register('CPC_AMH','SUM_SD',['6111','6112','6114'],[],'Achats revendus march.');
    _register('CPC_AMP','SUM_SD',['6121','6122','6124','6125','6126','6128','6129'],[],'Achats consommés MP & fourn.');
    _register('CPC_ACE','SUM_SD',['6131','6132','6133','6134','6135'],[],'Autres charges externes');
    _register('CPC_ACE2','SUM_SD',['6141','6142','6143','6144','6145','6146','6147','6148','6149'],[],'Autres charges ext. suite');
    _register('CPC_IMP','SUM_SD',['6161','6165','6167','6168'],[],'Impôts & taxes');
    _register('CPC_PER','SUM_SD',['6171','6174','6176','6177','6178'],[],'Charges de personnel');
    _register('CPC_DOT','SUM_SD',['6191','6192','6193','6194','6196','6198'],[],'Dot. exploitation');
    _register('CPC_TCE','REF',[],['CPC_AMH','CPC_AMP','CPC_ACE','CPC_ACE2','CPC_IMP','CPC_PER','CPC_DOT'],'TOTAL CHARGES EXPLOITATION');

    _register('CPC_REX','DIFF',[],['CPC_TPE','CPC_TCE'],'RÉSULTAT EXPLOITATION');

    /* Financier */
    _register('CPC_PFI','SUM_SC',['7311','7313','7318','7321','7325','7381','7382','7385','7386','7388','7391','7396'],[],'Produits financiers');
    _register('CPC_CFI','SUM_SD',['6311','6313','6318','6321','6325','6381','6382','6385','6386','6388','6391','6393','6396'],[],'Charges financières');
    _register('CPC_RFI','DIFF',[],['CPC_PFI','CPC_CFI'],'RÉSULTAT FINANCIER');

    _register('CPC_RCO','REF',[],['CPC_REX','CPC_RFI'],'RÉSULTAT COURANT');

    /* Non courant */
    _register('CPC_PNC','SUM_SC',['7511','7513','7518','7581','7585','7588','7591','7596'],[],'Produits non courants');
    _register('CPC_CNC','SUM_SD',['6511','6513','6518','6581','6585','6588','6591','6596'],[],'Charges non courantes');
    _register('CPC_RNC','DIFF',[],['CPC_PNC','CPC_CNC'],'RÉSULTAT NON COURANT');

    _register('CPC_RAI','REF',[],['CPC_RCO','CPC_RNC'],'RÉSULTAT AVANT IMPÔTS');
    _register('CPC_IIS','SUM_SD',['6701','6705','6708'],[],'Impôts sur les sociétés');
    _register('CPC_RNE','DIFF',[],['CPC_RAI','CPC_IIS'],'RÉSULTAT NET DE L\'EXERCICE');

    /* ─── ESG — État des Soldes de Gestion ─── */
    _register('ESG_MBR','DIFF',[],['CPC_VMH','CPC_AMH'],'Marge brute / ventes march.');
    _register('ESG_PRD','REF',[],['CPC_VBP','CPC_VST'],'Production de l\'exercice');
    _register('ESG_CNS','REF',[],['CPC_AMP','CPC_ACE','CPC_ACE2'],'Consommation de l\'exercice');
    _register('ESG_VAJ_BASE','REF',[],['ESG_MBR','ESG_PRD'],'VA base (MB + Production)');
    _register('ESG_VAJ','DIFF',[],['ESG_VAJ_BASE','ESG_CNS'],'Valeur ajoutée');
    _register('ESG_EBE','DIFF',[],['ESG_VAJ','CPC_IMP','CPC_PER'],'EBE (ou IBE)');

    return {
        getDefault:function(code){return _defaults[code]||null;},
        get:getFormula,
        compute:compute,
        ref:refString,
        parse:parseFormulaText,
        clearCache:clearCache,
        register:_register,
        allDefaults:function(){return Object.assign({},_defaults);}
    };
})();
/* ═══ 7. UI ENGINE — Excel-like rendering with formula references ═══ */
var UI = (function(){
    var _clientId, _exercice, _declId;
    var _currentPane = 'bilan';
    var _usingDemo = false;
    var _selectedCode = null;
    var _fxDraft = '';

    /* ─── FORMAT helpers ─── */
    function fmt(v){
        if(v===null||v===undefined||isNaN(v))return '-';
        return Number(v).toLocaleString('fr-FR',{minimumFractionDigits:2,maximumFractionDigits:2});
    }
    function fmtInt(v){
        if(v===null||v===undefined||isNaN(v))return '-';
        return Number(v).toLocaleString('fr-FR',{minimumFractionDigits:0,maximumFractionDigits:0});
    }

    /* ─── CELL RENDERER ─── */
    function cell(code, opts){
        opts=opts||{};
        var val = Formula.compute(code);
        var ref = Formula.ref(code);
        var ov = Storage.getOverride(code);
        var custom = Storage.getFormula(code);
        var cls = 'lf-cell';
        if(ov!==null) cls+=' lf-override';
        if(custom) cls+=' lf-configured';
        if(_selectedCode===code) cls+=' lf-cell-selected';
        if(opts.css) cls+=' '+opts.css;
        var h = '<td class="'+cls+'" data-code="'+code+'" onclick="LiasseFiscale.UI.selectCell(\''+code+'\')" ondblclick="LiasseFiscale.UI.focusFormulaBar()" title="'+_escHtml(ref)+'">';
        h+='<div class="lf-cell-val">'+fmt(val)+'</div>';
        if(ref) h+='<div class="lf-cell-ref">'+_escHtml(ref)+'</div>';
        h+='</td>';
        return h;
    }

    function cellVal(v, css){
        var c = css?' class="'+css+'"':'';
        return '<td'+c+'>'+fmt(v)+'</td>';
    }

    function cellText(t, css){
        var c = css?' class="'+css+'"':'';
        return '<td'+c+'>'+_escHtml(t)+'</td>';
    }

    function _escHtml(s){
        if(!s)return '';
        return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

    /* ─── EXCEL FORMULA BAR ─── */
    function selectCell(code){
        var f = Formula.get(code) || Formula.getDefault(code);
        if(!f && Storage.getOverride(code)===null) return;
        _selectedCode = code;
        _fxDraft = Formula.ref(code) || '';
        var nameEl = document.getElementById('lf-fx-name');
        var inputEl = document.getElementById('lf-fx-input');
        var valEl = document.getElementById('lf-fx-value');
        if(nameEl) nameEl.value = code;
        if(inputEl) inputEl.value = _fxDraft;
        if(valEl) valEl.textContent = fmt(Formula.compute(code));
        document.querySelectorAll('.lf-cell.lf-cell-selected').forEach(function(el){el.classList.remove('lf-cell-selected');});
        document.querySelectorAll('.lf-cell[data-code="'+code+'"]').forEach(function(el){el.classList.add('lf-cell-selected');});
        fillAccountPanel(code);
    }

    function focusFormulaBar(){
        var inputEl = document.getElementById('lf-fx-input');
        if(inputEl){ inputEl.focus(); inputEl.select(); }
    }

    function fillAccountPanel(code){
        var listEl = document.getElementById('lf-cfg-accounts');
        if(!listEl) return;
        var f = Formula.get(code) || Formula.getDefault(code);
        if(!f){ listEl.innerHTML = '<div class="text-muted small p-2">Aucune formule</div>'; return; }
        if(f.type === 'REF' || f.type === 'DIFF'){
            listEl.innerHTML = '<div class="lf-cfg-info p-2 small">Références: ' + _escHtml((f.refs||[]).join(', ')) + '</div>';
            return;
        }
        var usedAccts = {};
        (f.accounts||[]).forEach(function(a){usedAccts[a]=true;});
        var balAll = Balance.getAll();
        var relevantClass = _guessClassFromCode(code);
        var filtered = balAll;
        if(relevantClass > 0){
            filtered = balAll.filter(function(b){return parseInt(b.account[0])===relevantClass;});
        }
        if(filtered.length < 3) filtered = balAll;
        var html = '';
        filtered.forEach(function(b){
            var checked = usedAccts[b.account] ? ' checked' : '';
            html += '<label class="lf-acct-row">';
            html += '<input type="checkbox" value="'+b.account+'"'+checked+'>';
            html += '<span class="lf-acct-code">'+b.account+'</span>';
            html += '<span class="lf-acct-name">'+_escHtml(b.name||PCM.name(b.account))+'</span>';
            html += '<span class="lf-acct-solde">'+fmt(b.soldeD-b.soldeC)+'</span>';
            html += '</label>';
        });
        listEl.innerHTML = html || '<div class="text-muted small p-2">Aucun compte en balance</div>';
    }

    function _guessClassFromCode(code){
        if(code.indexOf('BA_')===0) return 0;
        if(code.indexOf('BP_')===0) return 0;
        if(code.indexOf('CPC_V')===0||code.indexOf('CPC_CA')===0||code.indexOf('CPC_P')===0||code.indexOf('CPC_R')===0) return 7;
        if(code.indexOf('CPC_A')===0||code.indexOf('CPC_C')===0||code.indexOf('CPC_I')===0||code.indexOf('CPC_D')===0) return 6;
        return 0;
    }

    function applyFormulaBar(){
        if(!_selectedCode) return;
        var inputEl = document.getElementById('lf-fx-input');
        var text = inputEl ? inputEl.value : '';
        var f = Formula.get(_selectedCode) || Formula.getDefault(_selectedCode);
        var parsed = Formula.parse(text, f ? f.type : 'SUM_SD');

        /* Also apply checkbox selection if panel open and SUM-type */
        var listEl = document.getElementById('lf-cfg-accounts');
        var panel = document.getElementById('lf-acct-panel');
        var panelOpen = panel && !panel.classList.contains('d-none');
        if(panelOpen && listEl && f && f.type !== 'REF' && f.type !== 'DIFF'){
            var checks = listEl.querySelectorAll('input[type=checkbox]');
            var newAccts = [];
            checks.forEach(function(ch){if(ch.checked) newAccts.push(ch.value);});
            if(newAccts.length && (!parsed || parsed.kind==='clear' || (text||'').trim()===(_fxDraft||'').trim())){
                var def = Formula.getDefault(_selectedCode);
                var defAccts = def ? (def.accounts||[]).slice().sort().join(',') : '';
                var newStr = newAccts.slice().sort().join(',');
                if(newStr !== defAccts){
                    Storage.setFormula(_selectedCode, { type:f.type, accounts:newAccts, refs:f.refs||[], label:f.label||_selectedCode });
                } else {
                    Storage.removeFormula(_selectedCode);
                }
                Storage.removeOverride(_selectedCode);
                Formula.clearCache();
                onPaneShown(_currentPane);
                selectCell(_selectedCode);
                return;
            }
        }

        if(!parsed){
            if(typeof toastr!=='undefined') toastr.error('Formule non reconnue');
            return;
        }
        if(parsed.kind==='clear'){
            Storage.removeOverride(_selectedCode);
            Storage.removeFormula(_selectedCode);
        } else if(parsed.kind==='override'){
            Storage.setOverride(_selectedCode, parsed.value);
        } else if(parsed.kind==='formula'){
            Storage.removeOverride(_selectedCode);
            var defF = Formula.getDefault(_selectedCode);
            var same = defF && defF.type===parsed.formula.type
                && JSON.stringify((defF.accounts||[]).slice().sort())===JSON.stringify((parsed.formula.accounts||[]).slice().sort())
                && JSON.stringify(defF.refs||[])===JSON.stringify(parsed.formula.refs||[]);
            if(same) Storage.removeFormula(_selectedCode);
            else {
                parsed.formula.label = (defF && defF.label) || _selectedCode;
                Storage.setFormula(_selectedCode, parsed.formula);
            }
        }
        Formula.clearCache();
        onPaneShown(_currentPane);
        selectCell(_selectedCode);
    }

    function cancelFormulaBar(){
        if(!_selectedCode) return;
        var inputEl = document.getElementById('lf-fx-input');
        if(inputEl) inputEl.value = Formula.ref(_selectedCode) || '';
        fillAccountPanel(_selectedCode);
    }

    function resetConfig(){
        if(!_selectedCode) return;
        Storage.removeFormula(_selectedCode);
        Storage.removeOverride(_selectedCode);
        Formula.clearCache();
        onPaneShown(_currentPane);
        selectCell(_selectedCode);
    }

    function toggleAccountPanel(){
        var panel = document.getElementById('lf-acct-panel');
        if(!panel) return;
        panel.classList.toggle('d-none');
        if(!panel.classList.contains('d-none') && _selectedCode) fillAccountPanel(_selectedCode);
    }

    function bindFormulaBar(){
        var applyBtn = document.getElementById('lf-fx-apply');
        var cancelBtn = document.getElementById('lf-fx-cancel');
        var resetBtn = document.getElementById('lf-fx-reset');
        var toggleBtn = document.getElementById('lf-fx-toggle-accts');
        var closePanel = document.getElementById('lf-acct-panel-close');
        var inputEl = document.getElementById('lf-fx-input');
        if(applyBtn) applyBtn.onclick = applyFormulaBar;
        if(cancelBtn) cancelBtn.onclick = cancelFormulaBar;
        if(resetBtn) resetBtn.onclick = resetConfig;
        if(toggleBtn) toggleBtn.onclick = toggleAccountPanel;
        if(closePanel) closePanel.onclick = function(){ var p=document.getElementById('lf-acct-panel'); if(p) p.classList.add('d-none'); };
        if(inputEl){
            inputEl.addEventListener('keydown', function(e){
                if(e.key==='Enter'){ e.preventDefault(); applyFormulaBar(); }
                if(e.key==='Escape'){ e.preventDefault(); cancelFormulaBar(); }
            });
        }
    }

    /* Legacy aliases */
    function openConfig(code){ selectCell(code); focusFormulaBar(); }
    function saveConfig(){ applyFormulaBar(); }
    function closeConfig(){ cancelFormulaBar(); }
    /* ─── RENDER: JOURNAL ─── */
    function renderJournal(filter){
        var el = document.getElementById('pane-journal');
        if(!el) return;
        var entries = Journal.getAll();
        var jCodes = Journal.getJournalCodes();
        filter = filter || '';

        if(filter) entries = entries.filter(function(e){return e.journal===filter;});

        var h = '<div class="lf-toolbar">';
        h+='<select id="lf-jnl-filter" onchange="LiasseFiscale.UI.filterJournal(this.value)" class="lf-select">';
        h+='<option value="">Tous les journaux</option>';
        jCodes.forEach(function(j){h+='<option value="'+j+'"'+(j===filter?' selected':'')+'>'+j+'</option>';});
        h+='</select>';
        h+='<span class="lf-toolbar-info">'+entries.length+' écritures</span>';
        h+='</div>';

        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Date</th><th>Pièce</th><th>Jnl</th><th>Compte</th><th>Libellé</th><th class="text-end">Débit</th><th class="text-end">Crédit</th></tr></thead>';
        h+='<tbody>';
        var tD=0, tC=0;
        entries.forEach(function(e,i){
            h+='<tr>';
            h+='<td class="lf-row-num">'+(i+1)+'</td>';
            h+='<td>'+e.date+'</td>';
            h+='<td><strong>'+e.piece+'</strong></td>';
            h+='<td><span class="badge bg-secondary">'+e.journal+'</span></td>';
            h+='<td class="lf-acct-code">'+e.account+' <small class="text-muted">'+_escHtml((PCM.get(e.account)||{}).n||'')+'</small></td>';
            h+='<td>'+_escHtml(e.label)+'</td>';
            h+='<td class="text-end'+(e.debit?'':' text-muted')+'">'+(e.debit?fmt(e.debit):'-')+'</td>';
            h+='<td class="text-end'+(e.credit?'':' text-muted')+'">'+(e.credit?fmt(e.credit):'-')+'</td>';
            h+='</tr>';
            tD+=e.debit; tC+=e.credit;
        });
        h+='</tbody><tfoot><tr class="lf-total-row"><td class="lf-row-num"></td><td colspan="5"><strong>TOTAUX</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(tD)+'</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(tC)+'</strong></td></tr></tfoot>';
        h+='</table></div>';
        el.innerHTML = h;
    }

    /* ─── RENDER: GRAND LIVRE ─── */
    function renderGrandLivre(filterAcct){
        var el = document.getElementById('pane-grand-livre');
        if(!el) return;
        var accts = Ledger.getAccounts();
        filterAcct = filterAcct || '';

        var h = '<div class="lf-toolbar">';
        h+='<input type="text" id="lf-gl-filter" class="lf-input" placeholder="Filtrer par compte..." value="'+filterAcct+'" onkeyup="LiasseFiscale.UI.filterGL(this.value)">';
        h+='<span class="lf-toolbar-info">'+accts.length+' comptes</span>';
        h+='</div>';

        var shown = accts;
        if(filterAcct) shown = accts.filter(function(a){return a.indexOf(filterAcct)===0 || PCM.name(a).toLowerCase().indexOf(filterAcct.toLowerCase())>=0;});

        shown.forEach(function(a){
            var ac = Ledger.getAccount(a);
            var solde = ac.totalD - ac.totalC;
            h+='<div class="lf-gl-account">';
            h+='<div class="lf-gl-header"><span class="lf-acct-code">'+a+'</span> <span>'+_escHtml((PCM.get(a)||{}).n||'')+'</span>';
            h+='<span class="float-end">Solde: <strong class="'+(solde>=0?'text-primary':'text-danger')+'">'+fmt(solde)+'</strong></span></div>';
            h+='<table class="lf-table lf-table-sm">';
            h+='<thead><tr><th>Date</th><th>Pièce</th><th>Libellé</th><th class="text-end">Débit</th><th class="text-end">Crédit</th></tr></thead><tbody>';
            var rD=0,rC=0;
            ac.entries.forEach(function(e){
                rD+=e.debit; rC+=e.credit;
                h+='<tr><td>'+e.date+'</td><td>'+e.piece+'</td><td>'+_escHtml(e.label)+'</td>';
                h+='<td class="text-end">'+(e.debit?fmt(e.debit):'-')+'</td>';
                h+='<td class="text-end">'+(e.credit?fmt(e.credit):'-')+'</td></tr>';
            });
            h+='</tbody><tfoot><tr class="lf-total-row"><td colspan="3"><strong>Total</strong></td>';
            h+='<td class="text-end"><strong>'+fmt(rD)+'</strong></td><td class="text-end"><strong>'+fmt(rC)+'</strong></td></tr></tfoot>';
            h+='</table></div>';
        });

        el.innerHTML = h;
    }

    /* ─── RENDER: BALANCE ─── */
    function renderBalance(){
        var el = document.getElementById('pane-balance');
        if(!el) return;
        var lines = Balance.getAll();

        var h = '<div class="lf-kpi-bar">';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Total Débit</span><span class="lf-kpi-value">'+fmt(Balance.totalDebit())+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Total Crédit</span><span class="lf-kpi-value">'+fmt(Balance.totalCredit())+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Soldes Débit</span><span class="lf-kpi-value">'+fmt(Balance.totalSD())+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Soldes Crédit</span><span class="lf-kpi-value">'+fmt(Balance.totalSC())+'</span></div>';
        h+='</div>';

        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Compte</th><th>Intitulé</th><th class="text-end">Mouvement Débit</th><th class="text-end">Mouvement Crédit</th><th class="text-end">Solde Débit</th><th class="text-end">Solde Crédit</th></tr></thead>';
        h+='<tbody>';
        var currentClass = 0;
        lines.forEach(function(l,i){
            var cls = parseInt(l.account[0]);
            if(cls !== currentClass){
                currentClass = cls;
                var clsNames = {1:'Comptes de Financement Permanent',2:'Comptes d\'Actif Immobilisé',3:'Comptes d\'Actif Circulant',4:'Comptes de Passif Circulant',5:'Comptes de Trésorerie',6:'Comptes de Charges',7:'Comptes de Produits'};
                h+='<tr class="lf-section-row"><td class="lf-row-num"></td><td colspan="6"><strong>CLASSE '+cls+' — '+(clsNames[cls]||'')+'</strong></td></tr>';
            }
            h+='<tr>';
            h+='<td class="lf-row-num">'+(i+1)+'</td>';
            h+='<td class="lf-acct-code">'+l.account+'</td>';
            h+='<td>'+_escHtml((PCM.get(l.account)||{}).n||l.account)+'</td>';
            h+='<td class="text-end">'+fmt(l.debit)+'</td>';
            h+='<td class="text-end">'+fmt(l.credit)+'</td>';
            h+='<td class="text-end'+(l.soldeD?'':' text-muted')+'">'+fmt(l.soldeD)+'</td>';
            h+='<td class="text-end'+(l.soldeC?'':' text-muted')+'">'+fmt(l.soldeC)+'</td>';
            h+='</tr>';
        });
        h+='</tbody><tfoot><tr class="lf-grand-total"><td class="lf-row-num"></td><td colspan="2"><strong>TOTAUX GÉNÉRAUX</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(Balance.totalDebit())+'</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(Balance.totalCredit())+'</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(Balance.totalSD())+'</strong></td>';
        h+='<td class="text-end"><strong>'+fmt(Balance.totalSC())+'</strong></td>';
        h+='</tr></tfoot></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: BILAN — Standard CGNC Moroccan layout ─── */
    function renderBilan(){
        var el = document.getElementById('pane-bilan');
        if(!el) return;
        Formula.clearCache();

        var h = '<div class="lf-split-view">';

        /* ── ACTIF ── */
        h+='<div class="lf-split-panel">';
        h+='<h6 class="lf-panel-title">ACTIF</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">Réf</th><th>A C T I F</th><th class="text-end">Brut</th><th class="text-end">Amort./Prov.</th><th class="text-end">Net</th></tr></thead>';
        h+='<tbody>';

        /* Section: ACTIF IMMOBILISÉ */
        h+='<tr class="lf-section-row"><td class="lf-row-num">AI</td><td colspan="4"><strong>ACTIF IMMOBILISÉ</strong></td></tr>';

        var actifRows = [
            {ref:'AI01',label:'Immob. en non-valeurs',brut:'BA_IMN',amort:'BA_AMN'},
            {ref:'AI02',label:'Immob. incorporelles',brut:'BA_IIC',amort:'BA_AII'},
            {ref:'AI03',label:'Terrains',brut:'BA_TER',amort:'BA_ATE'},
            {ref:'AI04',label:'Constructions',brut:'BA_CON',amort:'BA_ACO'},
            {ref:'AI05',label:'Install. tech. mat. & outillage',brut:'BA_ITR',amort:'BA_AIT'},
            {ref:'AI06',label:'Matériel de transport',brut:'BA_MTR',amort:'BA_AMT'},
            {ref:'AI07',label:'Mobilier, mat. bureau & info.',brut:'BA_MMB',amort:'BA_AMM'},
            {ref:'AI08',label:'Autres immob. corporelles',brut:'BA_AIC',amort:null},
            {ref:'AI09',label:'Immob. corp. en cours',brut:'BA_IEC',amort:null},
            {ref:'AI10',label:'Prêts immobilisés',brut:'BA_PRT',amort:null},
            {ref:'AI11',label:'Autres créances financières',brut:'BA_AIF',amort:null},
            {ref:'AI12',label:'Titres de participation',brut:'BA_TPR',amort:null}
        ];

        actifRows.forEach(function(r){
            var brut = Formula.compute(r.brut);
            var amort = r.amort ? Formula.compute(r.amort) : 0;
            var net = brut - amort;
            h+='<tr>';
            h+='<td class="lf-row-num">'+r.ref+'</td>';
            h+='<td>'+_escHtml(r.label)+'</td>';
            h+=cell(r.brut);
            if(r.amort) h+=cell(r.amort);
            else h+='<td class="text-muted text-center">-</td>';
            h+=cellVal(net, 'text-end fw-bold');
            h+='</tr>';
        });

        /* Total Actif Immobilisé */
        var tAI_brut = Formula.compute('BA_TIMM');
        var tAI_amort = Formula.compute('BA_TAMM');
        h+='<tr class="lf-total-row">';
        h+='<td class="lf-row-num">TAI</td><td><strong>TOTAL ACTIF IMMOBILISÉ</strong></td>';
        h+=cell('BA_TIMM');
        h+=cell('BA_TAMM');
        h+=cellVal(tAI_brut - tAI_amort, 'text-end fw-bold');
        h+='</tr>';

        /* ACTIF CIRCULANT */
        h+='<tr class="lf-section-row"><td class="lf-row-num">AC</td><td colspan="4"><strong>ACTIF CIRCULANT</strong></td></tr>';

        var circRows = [
            {ref:'AC01',label:'Stocks',code:'BA_STK'},
            {ref:'AC02',label:'Clients & rattachés',code:'BA_CLI'},
            {ref:'AC03',label:'Personnel débiteur',code:'BA_PER'},
            {ref:'AC04',label:'État débiteur',code:'BA_ETA'},
            {ref:'AC05',label:'Comptes assoc. & divers déb.',code:'BA_CDA'}
        ];

        circRows.forEach(function(r){
            h+='<tr><td class="lf-row-num">'+r.ref+'</td><td>'+_escHtml(r.label)+'</td>';
            h+=cell(r.code);
            h+='<td class="text-muted text-center">-</td>';
            h+=cell(r.code);
            h+='</tr>';
        });

        h+='<tr class="lf-total-row"><td class="lf-row-num">TAC</td><td><strong>TOTAL ACTIF CIRCULANT</strong></td>';
        h+=cell('BA_TCIR');
        h+='<td class="text-muted text-center">-</td>';
        h+=cell('BA_TCIR');
        h+='</tr>';

        /* TRÉSORERIE ACTIF */
        h+='<tr class="lf-section-row"><td class="lf-row-num">TA</td><td colspan="4"><strong>TRÉSORERIE - ACTIF</strong></td></tr>';
        h+='<tr><td class="lf-row-num">TA01</td><td>Titres & valeurs de placement</td>';
        h+=cell('BA_TVP'); h+='<td class="text-muted text-center">-</td>'; h+=cell('BA_TVP'); h+='</tr>';
        h+='<tr><td class="lf-row-num">TA02</td><td>Banques, TG & Caisse</td>';
        h+=cell('BA_BQA'); h+='<td class="text-muted text-center">-</td>'; h+=cell('BA_BQA'); h+='</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TTA</td><td><strong>TOTAL TRÉSORERIE ACTIF</strong></td>';
        h+=cell('BA_TTRA'); h+='<td class="text-muted text-center">-</td>'; h+=cell('BA_TTRA'); h+='</tr>';

        /* TOTAL GÉNÉRAL ACTIF */
        var totalActif = Formula.compute('BA_TIMM') - Formula.compute('BA_TAMM') + Formula.compute('BA_TCIR') + Formula.compute('BA_TTRA');
        h+='<tr class="lf-grand-total"><td class="lf-row-num">TGA</td><td><strong>TOTAL GÉNÉRAL ACTIF</strong></td>';
        h+=cell('BA_TOTAL');
        h+=cell('BA_TAMM');
        h+=cellVal(totalActif, 'text-end fw-bold');
        h+='</tr>';

        h+='</tbody></table></div></div>';

        /* ── PASSIF ── */
        h+='<div class="lf-split-panel">';
        h+='<h6 class="lf-panel-title">PASSIF</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">Réf</th><th>P A S S I F</th><th class="text-end">Exercice N</th></tr></thead>';
        h+='<tbody>';

        h+='<tr class="lf-section-row"><td class="lf-row-num">FP</td><td colspan="2"><strong>FINANCEMENT PERMANENT</strong></td></tr>';

        var fpRows = [
            {ref:'FP01',label:'Capital social',code:'BP_CAP'},
            {ref:'FP02',label:'Réserves',code:'BP_RES'},
            {ref:'FP03',label:'Report à nouveau',code:'BP_REP'},
            {ref:'FP04',label:'Résultat net',code:'BP_RNE'}
        ];
        fpRows.forEach(function(r){
            h+='<tr><td class="lf-row-num">'+r.ref+'</td><td>'+_escHtml(r.label)+'</td>';
            h+=cell(r.code); h+='</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num">TFP</td><td><strong>Total Fonds Propres</strong></td>'; h+=cell('BP_TFP'); h+='</tr>';

        h+='<tr><td class="lf-row-num">FP05</td><td>Subventions investissement</td>'; h+=cell('BP_SUB'); h+='</tr>';
        h+='<tr><td class="lf-row-num">FP06</td><td>Provisions réglementées</td>'; h+=cell('BP_PRV'); h+='</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TCP</td><td><strong>Total Capitaux Propres</strong></td>'; h+=cell('BP_TCP'); h+='</tr>';

        h+='<tr><td class="lf-row-num">FP07</td><td>Dettes de financement</td>'; h+=cell('BP_EMP'); h+='</tr>';
        h+='<tr><td class="lf-row-num">FP08</td><td>Provisions durables</td>'; h+=cell('BP_PDD'); h+='</tr>';
        h+='<tr><td class="lf-row-num">FP09</td><td>Écarts de conversion</td>'; h+=cell('BP_ECT'); h+='</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TFI</td><td><strong>TOTAL FINANCEMENT PERMANENT</strong></td>'; h+=cell('BP_TFIN'); h+='</tr>';

        /* PASSIF CIRCULANT */
        h+='<tr class="lf-section-row"><td class="lf-row-num">PC</td><td colspan="2"><strong>PASSIF CIRCULANT</strong></td></tr>';

        var pcRows = [
            {ref:'PC01',label:'Fournisseurs & rattachés',code:'BP_FRS'},
            {ref:'PC02',label:'Personnel créditeur',code:'BP_PRS'},
            {ref:'PC03',label:'Organismes sociaux',code:'BP_OSO'},
            {ref:'PC04',label:'État créditeur',code:'BP_ETC'},
            {ref:'PC05',label:'Comptes assoc. & divers créd.',code:'BP_DIV'}
        ];
        pcRows.forEach(function(r){
            h+='<tr><td class="lf-row-num">'+r.ref+'</td><td>'+_escHtml(r.label)+'</td>'; h+=cell(r.code); h+='</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num">TPC</td><td><strong>TOTAL PASSIF CIRCULANT</strong></td>'; h+=cell('BP_TCIR'); h+='</tr>';

        /* TRÉSORERIE PASSIF */
        h+='<tr class="lf-section-row"><td class="lf-row-num">TP</td><td colspan="2"><strong>TRÉSORERIE - PASSIF</strong></td></tr>';
        h+='<tr><td class="lf-row-num">TP01</td><td>Crédits de trésorerie</td>'; h+=cell('BP_CTR'); h+='</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TTP</td><td><strong>TOTAL TRÉSORERIE PASSIF</strong></td>'; h+=cell('BP_TTRP'); h+='</tr>';

        /* TOTAL GÉNÉRAL */
        h+='<tr class="lf-grand-total"><td class="lf-row-num">TGP</td><td><strong>TOTAL GÉNÉRAL PASSIF</strong></td>'; h+=cell('BP_TOTAL'); h+='</tr>';

        h+='</tbody></table></div></div>';
        h+='</div>'; /* end split-view */

        /* Contrôle Actif = Passif */
        var tActifNet = totalActif;
        var tPassif = Formula.compute('BP_TOTAL');
        var diff = tActifNet - tPassif;
        h+='<div class="lf-kpi-bar mt-2">';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Total Actif Net</span><span class="lf-kpi-value">'+fmt(tActifNet)+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Total Passif</span><span class="lf-kpi-value">'+fmt(tPassif)+'</span></div>';
        h+='<div class="lf-kpi '+(Math.abs(diff)<1?'lf-kpi-ok':'lf-kpi-err')+'"><span class="lf-kpi-label">Écart</span><span class="lf-kpi-value">'+fmt(diff)+'</span></div>';
        h+='</div>';

        el.innerHTML = h;
    }
    /* ─── RENDER: CPC — Compte de Produits et Charges ─── */
    function renderCPC(){
        var el = document.getElementById('pane-cpc');
        if(!el) return;
        Formula.clearCache();

        var h = '<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">Réf</th><th>COMPTE DE PRODUITS ET CHARGES</th><th class="text-end">Montant N</th></tr></thead>';
        h+='<tbody>';

        /* I. PRODUITS D'EXPLOITATION */
        h+='<tr class="lf-section-row"><td class="lf-row-num">I</td><td colspan="2"><strong>PRODUITS D\'EXPLOITATION</strong></td></tr>';

        var peRows = [
            {ref:'PE01',label:'Ventes de marchandises (en l\'état)',code:'CPC_VMH'},
            {ref:'PE02',label:'Ventes de biens & services produits',code:'CPC_VBP'},
            {ref:'PE03',label:'Prestations de services',code:'CPC_VST'},
            {ref:'PE04',label:'Chiffre d\'affaires',code:'CPC_CA',total:true},
            {ref:'PE05',label:'Subventions d\'exploitation',code:'CPC_VSP'},
            {ref:'PE06',label:'Autres produits d\'exploitation',code:'CPC_APE'},
            {ref:'PE07',label:'Reprises d\'expl., transferts charges',code:'CPC_RPE'}
        ];
        peRows.forEach(function(r){
            h+='<tr'+(r.total?' class="lf-result-row"':'')+'>';
            h+='<td class="lf-row-num">'+r.ref+'</td><td>'+(r.total?'<strong>':'')+_escHtml(r.label)+(r.total?'</strong>':'')+'</td>';
            h+=cell(r.code); h+='</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num">TPE</td><td><strong>TOTAL PRODUITS D\'EXPLOITATION</strong></td>';
        h+=cell('CPC_TPE'); h+='</tr>';

        /* II. CHARGES D'EXPLOITATION */
        h+='<tr class="lf-section-row"><td class="lf-row-num">II</td><td colspan="2"><strong>CHARGES D\'EXPLOITATION</strong></td></tr>';

        var ceRows = [
            {ref:'CE01',label:'Achats revendus de marchandises',code:'CPC_AMH'},
            {ref:'CE02',label:'Achats consommés mat. & fournitures',code:'CPC_AMP'},
            {ref:'CE03',label:'Autres charges externes',code:'CPC_ACE'},
            {ref:'CE04',label:'Autres charges externes (suite)',code:'CPC_ACE2'},
            {ref:'CE05',label:'Impôts & taxes',code:'CPC_IMP'},
            {ref:'CE06',label:'Charges de personnel',code:'CPC_PER'},
            {ref:'CE07',label:'Dotations d\'exploitation',code:'CPC_DOT'}
        ];
        ceRows.forEach(function(r){
            h+='<tr><td class="lf-row-num">'+r.ref+'</td><td>'+_escHtml(r.label)+'</td>';
            h+=cell(r.code); h+='</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num">TCE</td><td><strong>TOTAL CHARGES D\'EXPLOITATION</strong></td>';
        h+=cell('CPC_TCE'); h+='</tr>';

        /* RÉSULTAT D'EXPLOITATION */
        h+='<tr class="lf-result-row"><td class="lf-row-num">III</td><td><strong>RÉSULTAT D\'EXPLOITATION (I - II)</strong></td>';
        h+=cell('CPC_REX'); h+='</tr>';

        /* IV. PRODUITS FINANCIERS */
        h+='<tr class="lf-section-row"><td class="lf-row-num">IV</td><td colspan="2"><strong>PRODUITS FINANCIERS</strong></td></tr>';
        h+='<tr><td class="lf-row-num">PF01</td><td>Produits financiers</td>';h+=cell('CPC_PFI');h+='</tr>';

        /* V. CHARGES FINANCIÈRES */
        h+='<tr class="lf-section-row"><td class="lf-row-num">V</td><td colspan="2"><strong>CHARGES FINANCIÈRES</strong></td></tr>';
        h+='<tr><td class="lf-row-num">CF01</td><td>Charges financières</td>';h+=cell('CPC_CFI');h+='</tr>';

        /* RÉSULTAT FINANCIER */
        h+='<tr class="lf-result-row"><td class="lf-row-num">VI</td><td><strong>RÉSULTAT FINANCIER (IV - V)</strong></td>';
        h+=cell('CPC_RFI'); h+='</tr>';

        /* RÉSULTAT COURANT */
        h+='<tr class="lf-result-row"><td class="lf-row-num">VII</td><td><strong>RÉSULTAT COURANT (III + VI)</strong></td>';
        h+=cell('CPC_RCO'); h+='</tr>';

        /* VIII. PRODUITS NON COURANTS */
        h+='<tr class="lf-section-row"><td class="lf-row-num">VIII</td><td colspan="2"><strong>PRODUITS NON COURANTS</strong></td></tr>';
        h+='<tr><td class="lf-row-num">PNC</td><td>Produits non courants</td>';h+=cell('CPC_PNC');h+='</tr>';

        /* IX. CHARGES NON COURANTES */
        h+='<tr class="lf-section-row"><td class="lf-row-num">IX</td><td colspan="2"><strong>CHARGES NON COURANTES</strong></td></tr>';
        h+='<tr><td class="lf-row-num">CNC</td><td>Charges non courantes</td>';h+=cell('CPC_CNC');h+='</tr>';

        /* RÉSULTAT NON COURANT */
        h+='<tr class="lf-result-row"><td class="lf-row-num">X</td><td><strong>RÉSULTAT NON COURANT (VIII - IX)</strong></td>';
        h+=cell('CPC_RNC'); h+='</tr>';

        /* RÉSULTAT AVANT IMPÔTS */
        h+='<tr class="lf-result-row"><td class="lf-row-num">XI</td><td><strong>RÉSULTAT AVANT IMPÔTS (VII + X)</strong></td>';
        h+=cell('CPC_RAI'); h+='</tr>';

        /* IMPÔT SUR LES SOCIÉTÉS */
        h+='<tr><td class="lf-row-num">XII</td><td>Impôt sur les sociétés</td>';h+=cell('CPC_IIS');h+='</tr>';

        /* RÉSULTAT NET */
        h+='<tr class="lf-grand-total"><td class="lf-row-num">XIII</td><td><strong>RÉSULTAT NET DE L\'EXERCICE (XI - XII)</strong></td>';
        h+=cell('CPC_RNE'); h+='</tr>';

        h+='</tbody></table></div>';

        /* KPI bar */
        h+='<div class="lf-kpi-bar mt-2">';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">CA</span><span class="lf-kpi-value">'+fmt(Formula.compute('CPC_CA'))+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Rés. Exploitation</span><span class="lf-kpi-value">'+fmt(Formula.compute('CPC_REX'))+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Rés. Net</span><span class="lf-kpi-value">'+fmt(Formula.compute('CPC_RNE'))+'</span></div>';
        h+='</div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: ESG — État des Soldes de Gestion ─── */
    function renderESG(){
        var el = document.getElementById('pane-esg');
        if(!el) return;
        Formula.clearCache();

        var h = '<h6 class="lf-panel-title">TABLEAU DE FORMATION DES RÉSULTATS (TFR)</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">Réf</th><th>SOLDES DE GESTION</th><th class="text-end">Montant N</th></tr></thead>';
        h+='<tbody>';

        var esgRows = [
            {ref:'SG1',label:'Ventes de marchandises',code:'CPC_VMH'},
            {ref:'SG2',label:'(-) Achats revendus march.',code:'CPC_AMH',neg:true},
            {ref:'SG3',label:'= MARGE BRUTE / ventes march.',code:'ESG_MBR',result:true},
            {ref:'',label:'',code:null,separator:true},
            {ref:'SG4',label:'Production de l\'exercice',code:'ESG_PRD'},
            {ref:'SG5',label:'(-) Consommation de l\'exercice',code:'ESG_CNS',neg:true},
            {ref:'SG6',label:'= VALEUR AJOUTÉE',code:'ESG_VAJ',result:true},
            {ref:'',label:'',code:null,separator:true},
            {ref:'SG7',label:'(+) Subventions d\'exploitation',code:'CPC_VSP'},
            {ref:'SG8',label:'(-) Impôts & taxes',code:'CPC_IMP',neg:true},
            {ref:'SG9',label:'(-) Charges de personnel',code:'CPC_PER',neg:true},
            {ref:'SG10',label:'= EBE (Excédent Brut d\'Exploitation)',code:'ESG_EBE',result:true},
            {ref:'',label:'',code:null,separator:true},
            {ref:'SG11',label:'Résultat d\'exploitation',code:'CPC_REX',result:true},
            {ref:'SG12',label:'Résultat financier',code:'CPC_RFI'},
            {ref:'SG13',label:'= Résultat courant',code:'CPC_RCO',result:true},
            {ref:'SG14',label:'Résultat non courant',code:'CPC_RNC'},
            {ref:'SG15',label:'(-) Impôt sur les sociétés',code:'CPC_IIS',neg:true},
            {ref:'SG16',label:'= RÉSULTAT NET',code:'CPC_RNE',grand:true}
        ];

        esgRows.forEach(function(r){
            if(r.separator){h+='<tr><td class="lf-row-num"></td><td colspan="2" style="height:6px;"></td></tr>';return;}
            var cls = r.grand?'lf-grand-total':r.result?'lf-result-row':'';
            h+='<tr'+(cls?' class="'+cls+'"':'')+'>';
            h+='<td class="lf-row-num">'+r.ref+'</td>';
            h+='<td>'+(r.result||r.grand?'<strong>':'')+_escHtml(r.label)+(r.result||r.grand?'</strong>':'')+'</td>';
            if(r.code) h+=cell(r.code); else h+='<td></td>';
            h+='</tr>';
        });

        h+='</tbody></table></div>';

        /* CAF */
        h+='<h6 class="lf-panel-title mt-3">CAPACITÉ D\'AUTOFINANCEMENT (CAF)</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">Réf</th><th>Éléments</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';
        h+='<tr><td class="lf-row-num">CF1</td><td>Résultat net</td>'; h+=cell('CPC_RNE'); h+='</tr>';
        h+='<tr><td class="lf-row-num">CF2</td><td>(+) Dotations d\'exploitation</td>'; h+=cell('CPC_DOT'); h+='</tr>';
        h+='<tr class="lf-result-row"><td class="lf-row-num">CAF</td><td><strong>= CAPACITÉ D\'AUTOFINANCEMENT</strong></td>';
        var caf = Formula.compute('CPC_RNE') + Formula.compute('CPC_DOT');
        h+=cellVal(caf,'text-end fw-bold');
        h+='</tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: TABLEAU DE FINANCEMENT ─── */
    function renderTF(){
        var el = document.getElementById('pane-tf');
        if(!el) return;
        Formula.clearCache();

        var caf = Formula.compute('CPC_RNE') + Formula.compute('CPC_DOT');

        var h = '<h6 class="lf-panel-title">TABLEAU DE FINANCEMENT — Synthèse des Masses</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>RESSOURCES STABLES</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';
        var cessions = Formula.compute('CPC_PNC');
        var dettesFin = Formula.compute('BP_EMP');
        var totalRes = caf + Math.max(0, cessions) + Math.max(0, dettesFin);
        h+='<tr><td class="lf-row-num">R1</td><td>Capacité d\'autofinancement (CAF)</td>'+cellVal(caf,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">R2</td><td>Produits de cession / non courants (approx.)</td>'+cell('CPC_PNC')+'</tr>';
        h+='<tr><td class="lf-row-num">R3</td><td>Fonds propres</td>'+cell('BP_TFP')+'</tr>';
        h+='<tr><td class="lf-row-num">R4</td><td>Dettes de financement</td>'+cell('BP_EMP')+'</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TR</td><td><strong>TOTAL RESSOURCES (synthèse)</strong></td>'+cellVal(totalRes,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div>';

        h+='<div class="lf-table-wrap mt-2"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>EMPLOIS STABLES</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';
        var invst = Formula.compute('BA_TIMM');
        var chargesNc = Formula.compute('CPC_CNC');
        h+='<tr><td class="lf-row-num">E1</td><td>Actif immobilisé (brut synthèse)</td>'+cell('BA_TIMM')+'</tr>';
        h+='<tr><td class="lf-row-num">E2</td><td>Charges non courantes (approx. emplois)</td>'+cell('CPC_CNC')+'</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">TE</td><td><strong>TOTAL EMPLOIS (synthèse)</strong></td>'+cellVal(invst+chargesNc,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div>';

        h+='<div class="lf-kpi-bar mt-2">';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">Δ Fonds de Roulement (approx.)</span><span class="lf-kpi-value">'+fmt(totalRes - invst - chargesNc)+'</span></div>';
        h+='</div>';

        el.innerHTML = h;
    }
    /* ─── RENDER: ETIC ─── */
    function renderETIC(){
        var el = document.getElementById('pane-etic');
        if(!el) return;

        var h = '<h6 class="lf-panel-title">ÉTAT DES INFORMATIONS COMPLÉMENTAIRES (ETIC)</h6>';

        /* A1. Principales Méthodes */
        h+='<div class="lf-etic-section"><h6>A1 — Principales méthodes d\'évaluation</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table"><tbody>';
        h+='<tr><td class="fw-bold" style="width:200px">Immobilisations corporelles</td><td>Coût historique d\'acquisition (CGNC)</td></tr>';
        h+='<tr><td class="fw-bold">Amortissements</td><td>Linéaire — Bâtiments:20 ans, ITMO:10 ans, Transport:5 ans, Mobilier/Info:5 ans</td></tr>';
        h+='<tr><td class="fw-bold">Stocks</td><td>CMUP (Coût Moyen Unitaire Pondéré)</td></tr>';
        h+='<tr><td class="fw-bold">Créances</td><td>Valeur nominale, provision pour dépréciation si risque de non-recouvrement</td></tr>';
        h+='</tbody></table></div></div>';

        /* A2. Dérogations */
        h+='<div class="lf-etic-section"><h6>A2 — Dérogations</h6>';
        h+='<p class="text-muted fst-italic">Aucune dérogation aux principes comptables fondamentaux.</p></div>';

        /* A3. Changements */
        h+='<div class="lf-etic-section"><h6>A3 — Changements de méthodes</h6>';
        h+='<p class="text-muted fst-italic">Aucun changement de méthode au cours de l\'exercice.</p></div>';

        /* B1. Détail immobilisations — brut fin = solde balance (N); début/acq/cessions N/A sans historique */
        h+='<div class="lf-etic-section"><h6>B1 — Tableau des immobilisations (soldes exercice)</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th>Nature</th><th class="text-end">Brut Début</th><th class="text-end">Acquisitions</th><th class="text-end">Cessions</th><th class="text-end">Brut Fin</th></tr></thead>';
        h+='<tbody>';
        var immRows = [
            {label:'Immob. en non-valeurs',code:'BA_IMN'},
            {label:'Immob. incorporelles',code:'BA_IIC'},
            {label:'Terrains',code:'BA_TER'},
            {label:'Constructions',code:'BA_CON'},
            {label:'ITMO',code:'BA_ITR'},
            {label:'Matériel de transport',code:'BA_MTR'},
            {label:'MMB & Info',code:'BA_MMB'},
            {label:'Autres immob. corp.',code:'BA_AIC'},
            {label:'Immob. en cours',code:'BA_IEC'},
            {label:'Prêts',code:'BA_PRT'},
            {label:'Titres participation',code:'BA_TPR'}
        ];
        var tBF=0;
        immRows.forEach(function(r){
            var v = Formula.compute(r.code); tBF+=v;
            h+='<tr><td>'+_escHtml(r.label)+'</td>'+cellVal(0,'text-end text-muted')+cellVal(v,'text-end')+cellVal(0,'text-end text-muted')+cellVal(v,'text-end')+'</tr>';
        });
        h+='<tr class="lf-total-row"><td><strong>TOTAL</strong></td>'+cellVal(0,'text-end')+cellVal(tBF,'text-end fw-bold')+cellVal(0,'text-end')+cellVal(tBF,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div></div>';

        /* B2. Tableau des amortissements */
        h+='<div class="lf-etic-section"><h6>B2 — Tableau des amortissements</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th>Nature</th><th class="text-end">Cumul Début</th><th class="text-end">Dotation Exercice</th><th class="text-end">Reprise</th><th class="text-end">Cumul Fin</th></tr></thead>';
        h+='<tbody>';
        var amRows = [
            {label:'Amort. non-valeurs',code:'BA_AMN'},
            {label:'Amort. immob. incorp.',code:'BA_AII'},
            {label:'Amort. constructions',code:'BA_ACO'},
            {label:'Amort. ITMO',code:'BA_AIT'},
            {label:'Amort. mat. transport',code:'BA_AMT'},
            {label:'Amort. MMB',code:'BA_AMM'}
        ];
        var tAF=0;
        amRows.forEach(function(r){
            var v = Formula.compute(r.code); tAF+=v;
            h+='<tr><td>'+_escHtml(r.label)+'</td>'+cellVal(0,'text-end text-muted')+cellVal(v,'text-end')+cellVal(0,'text-end text-muted')+cellVal(v,'text-end')+'</tr>';
        });
        h+='<tr class="lf-total-row"><td><strong>TOTAL AMORT.</strong></td>'+cellVal(0,'text-end')+cellVal(tAF,'text-end fw-bold')+cellVal(0,'text-end')+cellVal(tAF,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div></div>';

        h+='<div class="lf-etic-section"><h6>B3 — Provisions</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table"><tbody>';
        h+='<tr><td>Provisions durables</td>'+cell('BP_PDD')+'</tr>';
        h+='<tr><td>Provisions dépréciation actif circulant</td>'+cell('BA_CRA')+'</tr>';
        h+='</tbody></table></div></div>';

        h+='<div class="lf-etic-section"><h6>B4 — Créances</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table"><tbody>';
        h+='<tr><td>Clients & rattachés</td>'+cell('BA_CLI')+'</tr>';
        h+='<tr><td>État débiteur</td>'+cell('BA_ETA')+'</tr>';
        h+='<tr><td>Autres débiteurs</td>'+cell('BA_CDA')+'</tr>';
        h+='</tbody></table></div></div>';

        h+='<div class="lf-etic-section"><h6>B5 — Dettes</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table"><tbody>';
        h+='<tr><td>Fournisseurs</td>'+cell('BP_FRS')+'</tr>';
        h+='<tr><td>Personnel / organismes sociaux</td>'+cellVal(Formula.compute('BP_PRS')+Formula.compute('BP_OSO'),'text-end')+'</tr>';
        h+='<tr><td>État créditeur</td>'+cell('BP_ETC')+'</tr>';
        h+='<tr><td>Dettes de financement</td>'+cell('BP_EMP')+'</tr>';
        h+='</tbody></table></div></div>';

        h+='<div class="lf-etic-section"><h6>C2 — Affectation du résultat</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table"><tbody>';
        h+='<tr><td>Résultat net de l\'exercice</td>'+cell('CPC_RNE')+'</tr>';
        h+='<tr><td>Report à nouveau</td>'+cell('BP_REP')+'</tr>';
        h+='</tbody></table></div></div>';

        h+='<div class="lf-etic-section"><h6>Autres annexes</h6>';
        h+='<p class="text-muted small mb-0">B6–B9 / C1 / C3 : à compléter hors système (sûretés, effectifs, répartition capital, historiques multi-exercices).</p></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: DETAIL CPC ─── */
    function renderDetailCPC(){
        var el = document.getElementById('pane-detail-cpc');
        if(!el) return;
        Formula.clearCache();

        /* Build detailed breakdown by account */
        var h = '<h6 class="lf-panel-title">DÉTAIL DU CPC PAR COMPTE</h6>';

        var sections = [
            {title:'PRODUITS D\'EXPLOITATION',prefix:'7',filter:function(a){return a[0]==='7'&&parseInt(a.substr(0,2))<=71;}},
            {title:'CHARGES D\'EXPLOITATION',prefix:'6',filter:function(a){return a[0]==='6'&&parseInt(a.substr(0,2))<=61;}},
            {title:'CHARGES EXTERNES',prefix:'61',filter:function(a){return a.indexOf('61')===0&&parseInt(a.substr(0,3))>=613;}},
            {title:'PRODUITS FINANCIERS',prefix:'73',filter:function(a){return a.indexOf('73')===0||a.indexOf('739')===0;}},
            {title:'CHARGES FINANCIÈRES',prefix:'63',filter:function(a){return a.indexOf('63')===0;}},
            {title:'PRODUITS NON COURANTS',prefix:'75',filter:function(a){return a.indexOf('75')===0||a.indexOf('759')===0;}},
            {title:'CHARGES NON COURANTES',prefix:'65',filter:function(a){return a.indexOf('65')===0;}}
        ];

        sections.forEach(function(sec){
            var lines = Balance.getAll().filter(function(l){return sec.filter(l.account);});
            if(!lines.length) return;
            h+='<div class="lf-detail-section">';
            h+='<h6 class="lf-section-title">'+sec.title+'</h6>';
            h+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
            h+='<thead><tr><th>Compte</th><th>Intitulé</th><th class="text-end">Débit</th><th class="text-end">Crédit</th><th class="text-end">Solde</th></tr></thead>';
            h+='<tbody>';
            var tD=0,tC=0;
            lines.forEach(function(l){
                var s=l.debit-l.credit; tD+=l.debit; tC+=l.credit;
                h+='<tr><td class="lf-acct-code">'+l.account+'</td><td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
                h+='<td class="text-end">'+fmt(l.debit)+'</td><td class="text-end">'+fmt(l.credit)+'</td>';
                h+='<td class="text-end fw-bold '+(s>=0?'':'text-danger')+'">'+fmt(s)+'</td></tr>';
            });
            h+='</tbody><tfoot><tr class="lf-total-row"><td colspan="2"><strong>Sous-total</strong></td>';
            h+='<td class="text-end"><strong>'+fmt(tD)+'</strong></td><td class="text-end"><strong>'+fmt(tC)+'</strong></td>';
            h+='<td class="text-end"><strong>'+fmt(tD-tC)+'</strong></td></tr></tfoot>';
            h+='</table></div></div>';
        });

        el.innerHTML = h;
    }

    /* ─── RENDER: PASSAGE DU RÉSULTAT FISCAL ─── */
    function renderPassage(){
        var el = document.getElementById('pane-passage');
        if(!el) return;
        Formula.clearCache();

        var rne = Formula.compute('CPC_RNE');
        /* Simulated reintegrations & deductions */
        var reint = [
            {label:'Amendes & pénalités (non déductibles)',val:Formula.compute('CPC_CNC')*0.1},
            {label:'Dotations non déductibles',val:0},
            {label:'Charges non justifiées',val:0}
        ];
        var deduc = [
            {label:'Dividendes exonérés (abattement 100%)',val:0},
            {label:'Reprises provisions exonérées',val:0}
        ];
        var totR=0,totD=0;

        var h='<h6 class="lf-panel-title">PASSAGE DU RÉSULTAT COMPTABLE AU RÉSULTAT FISCAL</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Éléments</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';

        h+='<tr class="lf-result-row"><td class="lf-row-num">1</td><td><strong>Résultat net comptable</strong></td>'+cellVal(rne,'text-end fw-bold')+'</tr>';
        h+='<tr class="lf-section-row"><td class="lf-row-num"></td><td colspan="2"><strong>RÉINTÉGRATIONS (+)</strong></td></tr>';
        reint.forEach(function(r,i){
            totR+=r.val;
            h+='<tr><td class="lf-row-num">R'+(i+1)+'</td><td>'+_escHtml(r.label)+'</td>'+cellVal(r.val,'text-end')+'</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num"></td><td><strong>Total réintégrations</strong></td>'+cellVal(totR,'text-end fw-bold')+'</tr>';

        h+='<tr class="lf-section-row"><td class="lf-row-num"></td><td colspan="2"><strong>DÉDUCTIONS (-)</strong></td></tr>';
        deduc.forEach(function(d,i){
            totD+=d.val;
            h+='<tr><td class="lf-row-num">D'+(i+1)+'</td><td>'+_escHtml(d.label)+'</td>'+cellVal(d.val,'text-end')+'</tr>';
        });
        h+='<tr class="lf-total-row"><td class="lf-row-num"></td><td><strong>Total déductions</strong></td>'+cellVal(totD,'text-end fw-bold')+'</tr>';

        var rf = rne + totR - totD;
        h+='<tr class="lf-grand-total"><td class="lf-row-num">RF</td><td><strong>RÉSULTAT FISCAL</strong></td>'+cellVal(rf,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: AFFECTATION DU RÉSULTAT ─── */
    function renderAffectation(){
        var el = document.getElementById('pane-affectation');
        if(!el) return;
        Formula.clearCache();

        var rne = Formula.compute('CPC_RNE');
        var resLegal = Math.min(rne*0.05, Formula.compute('BP_CAP')*0.2);
        if(resLegal<0) resLegal=0;
        var dividendes = Math.max(0, (rne-resLegal)*0.4);
        var report = rne - resLegal - dividendes;

        var h = '<h6 class="lf-panel-title">PROPOSITION D\'AFFECTATION DU RÉSULTAT</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Éléments</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';
        h+='<tr class="lf-result-row"><td class="lf-row-num">1</td><td><strong>Résultat net de l\'exercice</strong></td>'+cellVal(rne,'text-end fw-bold')+'</tr>';
        h+='<tr><td class="lf-row-num">2</td><td>Report à nouveau antérieur</td>'+cellVal(0,'text-end text-muted')+'</tr>';
        h+='<tr class="lf-total-row"><td class="lf-row-num">3</td><td><strong>Montant à affecter</strong></td>'+cellVal(rne,'text-end fw-bold')+'</tr>';
        h+='<tr class="lf-section-row"><td class="lf-row-num"></td><td colspan="2"><strong>AFFECTATION</strong></td></tr>';
        h+='<tr><td class="lf-row-num">4</td><td>Réserve légale (5%, plafond 20% capital)</td>'+cellVal(resLegal,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">5</td><td>Dividendes</td>'+cellVal(dividendes,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">6</td><td>Report à nouveau</td>'+cellVal(report,'text-end')+'</tr>';
        h+='<tr class="lf-grand-total"><td class="lf-row-num">T</td><td><strong>Total affecté</strong></td>'+cellVal(resLegal+dividendes+report,'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }
    /* ─── RENDER: TVA DETAIL ─── */
    function renderTVADetail(){
        var el = document.getElementById('pane-tva');
        if(!el) return;

        var tvaCollectee = Balance.getAll().filter(function(l){return l.account.indexOf('4452')===0;});
        var tvaRecup = Balance.getAll().filter(function(l){return l.account.indexOf('3452')===0||l.account==='3455';});
        var tvaDue = Balance.getAll().filter(function(l){return l.account==='4456';});

        var h = '<h6 class="lf-panel-title">DÉTAIL TVA</h6>';

        function _tvaTable(title, lines){
            var html = '<div class="lf-detail-section"><h6 class="lf-section-title">'+title+'</h6>';
            html+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
            html+='<thead><tr><th>Compte</th><th>Intitulé</th><th class="text-end">Débit</th><th class="text-end">Crédit</th><th class="text-end">Solde</th></tr></thead>';
            html+='<tbody>';
            var total=0;
            lines.forEach(function(l){
                var s=l.soldeD-l.soldeC; total+=s;
                html+='<tr><td class="lf-acct-code">'+l.account+'</td><td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
                html+='<td class="text-end">'+fmt(l.debit)+'</td><td class="text-end">'+fmt(l.credit)+'</td>';
                html+='<td class="text-end fw-bold">'+fmt(s)+'</td></tr>';
            });
            html+='</tbody><tfoot><tr class="lf-total-row"><td colspan="4"><strong>Total</strong></td>';
            html+='<td class="text-end fw-bold">'+fmt(total)+'</td></tr></tfoot>';
            html+='</table></div></div>';
            return html;
        }

        h+=_tvaTable('TVA Collectée (facturée)', tvaCollectee);
        h+=_tvaTable('TVA Récupérable', tvaRecup);
        h+=_tvaTable('TVA Due', tvaDue);

        var coll=0,rec=0;
        tvaCollectee.forEach(function(l){coll+=l.credit-l.debit;});
        tvaRecup.forEach(function(l){rec+=l.debit-l.credit;});

        h+='<div class="lf-kpi-bar mt-2">';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">TVA Collectée</span><span class="lf-kpi-value">'+fmt(coll)+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">TVA Récupérable</span><span class="lf-kpi-value">'+fmt(rec)+'</span></div>';
        h+='<div class="lf-kpi"><span class="lf-kpi-label">TVA Nette</span><span class="lf-kpi-value">'+fmt(coll-rec)+'</span></div>';
        h+='</div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: IMMOBILISATIONS ─── */
    function renderImmob(){
        var el = document.getElementById('pane-immob');
        if(!el) return;

        var h = '<h6 class="lf-panel-title">TABLEAU DES IMMOBILISATIONS</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Compte</th><th>Nature</th><th class="text-end">Valeur Brute</th><th class="text-end">Amort. Cumulé</th><th class="text-end">Valeur Nette</th></tr></thead>';
        h+='<tbody>';

        var cls2 = Balance.getAll().filter(function(l){return l.account[0]==='2'&&l.account.indexOf('28')!==0&&l.account.indexOf('29')!==0;});
        var amorts = {};
        Balance.getAll().filter(function(l){return l.account.indexOf('28')===0;}).forEach(function(l){amorts[l.account]=l;});

        var tBrut=0, tAmort=0;
        cls2.forEach(function(l,i){
            var brut = l.soldeD;
            /* Find corresponding amort account: 28XX for 2XX */
            var amKey = '28' + l.account.substr(1);
            var am = amorts[amKey]; var amVal = am ? am.soldeC : 0;
            tBrut+=brut; tAmort+=amVal;
            h+='<tr><td class="lf-row-num">'+(i+1)+'</td><td class="lf-acct-code">'+l.account+'</td>';
            h+='<td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
            h+='<td class="text-end">'+fmt(brut)+'</td><td class="text-end">'+fmt(amVal)+'</td>';
            h+='<td class="text-end fw-bold">'+fmt(brut-amVal)+'</td></tr>';
        });
        h+='<tr class="lf-grand-total"><td class="lf-row-num"></td><td colspan="2"><strong>TOTAL</strong></td>';
        h+='<td class="text-end fw-bold">'+fmt(tBrut)+'</td><td class="text-end fw-bold">'+fmt(tAmort)+'</td>';
        h+='<td class="text-end fw-bold">'+fmt(tBrut-tAmort)+'</td></tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: AMORTISSEMENTS ─── */
    function renderAmort(){
        var el = document.getElementById('pane-amort');
        if(!el) return;

        var h = '<h6 class="lf-panel-title">TABLEAU DES AMORTISSEMENTS</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Compte</th><th>Nature</th><th class="text-end">Cumul Début</th><th class="text-end">Dotation</th><th class="text-end">Reprise</th><th class="text-end">Cumul Fin</th></tr></thead>';
        h+='<tbody>';

        var amLines = Balance.getAll().filter(function(l){return l.account.indexOf('28')===0;});
        var tCum=0;
        amLines.forEach(function(l,i){
            var cumFin = l.soldeC; tCum+=cumFin;
            h+='<tr><td class="lf-row-num">'+(i+1)+'</td><td class="lf-acct-code">'+l.account+'</td>';
            h+='<td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
            h+='<td class="text-end text-muted">'+fmt(0)+'</td><td class="text-end">'+fmt(cumFin)+'</td>';
            h+='<td class="text-end text-muted">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(cumFin)+'</td></tr>';
        });
        h+='<tr class="lf-grand-total"><td class="lf-row-num"></td><td colspan="2"><strong>TOTAL</strong></td>';
        h+='<td class="text-end">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(tCum)+'</td>';
        h+='<td class="text-end">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(tCum)+'</td></tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: PROVISIONS ─── */
    function renderProvisions(){
        var el = document.getElementById('pane-provisions');
        if(!el) return;

        var h = '<h6 class="lf-panel-title">TABLEAU DES PROVISIONS</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Compte</th><th>Nature</th><th class="text-end">Début</th><th class="text-end">Dotation</th><th class="text-end">Reprise</th><th class="text-end">Fin</th></tr></thead>';
        h+='<tbody>';

        var provLines = Balance.getAll().filter(function(l){return l.account.indexOf('15')===0||l.account.indexOf('29')===0||l.account.indexOf('39')===0||l.account.indexOf('49')===0||l.account.indexOf('349')===0;});
        var tFin=0;
        provLines.forEach(function(l,i){
            var fin=l.soldeC; tFin+=fin;
            h+='<tr><td class="lf-row-num">'+(i+1)+'</td><td class="lf-acct-code">'+l.account+'</td>';
            h+='<td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
            h+='<td class="text-end text-muted">'+fmt(0)+'</td><td class="text-end">'+fmt(fin)+'</td>';
            h+='<td class="text-end text-muted">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(fin)+'</td></tr>';
        });
        h+='<tr class="lf-grand-total"><td class="lf-row-num"></td><td colspan="2"><strong>TOTAL</strong></td>';
        h+='<td class="text-end">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(tFin)+'</td>';
        h+='<td class="text-end">'+fmt(0)+'</td><td class="text-end fw-bold">'+fmt(tFin)+'</td></tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: CRÉANCES & DETTES ─── */
    function renderCreances(){
        var el = document.getElementById('pane-creances');
        if(!el) return;

        var h = '<h6 class="lf-panel-title">ÉTAT DES CRÉANCES ET DETTES</h6>';

        /* Créances */
        h+='<div class="lf-detail-section"><h6 class="lf-section-title">CRÉANCES</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
        h+='<thead><tr><th>Compte</th><th>Nature</th><th class="text-end">Montant</th><th class="text-end">- 1 an</th><th class="text-end">+ 1 an</th></tr></thead>';
        h+='<tbody>';
        var creances = Balance.getAll().filter(function(l){return l.account[0]==='3'&&l.soldeD>0;});
        var tCr=0;
        creances.forEach(function(l){
            tCr+=l.soldeD;
            h+='<tr><td class="lf-acct-code">'+l.account+'</td><td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
            h+='<td class="text-end">'+fmt(l.soldeD)+'</td><td class="text-end">'+fmt(l.soldeD)+'</td><td class="text-end text-muted">-</td></tr>';
        });
        h+='<tr class="lf-total-row"><td colspan="2"><strong>Total Créances</strong></td>';
        h+='<td class="text-end fw-bold">'+fmt(tCr)+'</td><td class="text-end fw-bold">'+fmt(tCr)+'</td><td class="text-end">-</td></tr>';
        h+='</tbody></table></div></div>';

        /* Dettes */
        h+='<div class="lf-detail-section"><h6 class="lf-section-title">DETTES</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
        h+='<thead><tr><th>Compte</th><th>Nature</th><th class="text-end">Montant</th><th class="text-end">- 1 an</th><th class="text-end">+ 1 an</th></tr></thead>';
        h+='<tbody>';
        var dettes = Balance.getAll().filter(function(l){return l.account[0]==='4'&&l.soldeC>0;});
        var tDt=0;
        dettes.forEach(function(l){
            tDt+=l.soldeC;
            h+='<tr><td class="lf-acct-code">'+l.account+'</td><td>'+_escHtml((PCM.get(l.account)||{}).n||'')+'</td>';
            h+='<td class="text-end">'+fmt(l.soldeC)+'</td><td class="text-end">'+fmt(l.soldeC)+'</td><td class="text-end text-muted">-</td></tr>';
        });
        h+='<tr class="lf-total-row"><td colspan="2"><strong>Total Dettes</strong></td>';
        h+='<td class="text-end fw-bold">'+fmt(tDt)+'</td><td class="text-end fw-bold">'+fmt(tDt)+'</td><td class="text-end">-</td></tr>';
        h+='</tbody></table></div></div>';

        el.innerHTML = h;
    }

    /* ─── RENDER: ÉTAT 9421 — Détail IS ─── */
    function render9421(){
        var el = document.getElementById('pane-9421');
        if(!el) return;
        Formula.clearCache();

        var rne = Formula.compute('CPC_RNE');
        var ca = Formula.compute('CPC_CA');
        var cm = Math.max(ca * 0.0075, 3000);
        var isCalc = Formula.compute('CPC_IIS');
        var acomptes = Formula.compute('BA_ETA') * 0.5; /* Simplified */

        var h = '<h6 class="lf-panel-title">ÉTAT 9421 — DÉCLARATION IS</h6>';
        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Rubrique</th><th class="text-end">Montant</th></tr></thead>';
        h+='<tbody>';
        h+='<tr><td class="lf-row-num">1</td><td>Chiffre d\'affaires HT</td>'+cellVal(ca,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">2</td><td>Résultat net comptable</td>'+cellVal(rne,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">3</td><td>Réintégrations fiscales</td>'+cellVal(0,'text-end text-muted')+'</tr>';
        h+='<tr><td class="lf-row-num">4</td><td>Déductions fiscales</td>'+cellVal(0,'text-end text-muted')+'</tr>';
        h+='<tr class="lf-result-row"><td class="lf-row-num">5</td><td><strong>Résultat fiscal</strong></td>'+cellVal(rne,'text-end fw-bold')+'</tr>';
        h+='<tr><td class="lf-row-num">6</td><td>Déficits reportables imputés</td>'+cellVal(0,'text-end text-muted')+'</tr>';
        h+='<tr class="lf-result-row"><td class="lf-row-num">7</td><td><strong>Base imposable</strong></td>'+cellVal(rne,'text-end fw-bold')+'</tr>';
        h+='<tr class="lf-section-row"><td class="lf-row-num"></td><td colspan="2"><strong>BARÈME IS PROGRESSIF 2024</strong></td></tr>';
        h+='<tr><td class="lf-row-num">8a</td><td>≤ 300 000 DH : 10%</td>'+cellVal(Math.min(rne,300000)*0.10,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">8b</td><td>300 001 — 1 000 000 : 20%</td>'+cellVal(Math.max(0,Math.min(rne-300000,700000))*0.20,'text-end')+'</tr>';
        h+='<tr><td class="lf-row-num">8c</td><td>> 1 000 000 : 31%</td>'+cellVal(Math.max(0,rne-1000000)*0.31,'text-end')+'</tr>';
        h+='<tr class="lf-result-row"><td class="lf-row-num">9</td><td><strong>IS calculé</strong></td>'+cellVal(isCalc,'text-end fw-bold')+'</tr>';
        h+='<tr><td class="lf-row-num">10</td><td>Cotisation minimale (0,75% CA, min 3 000)</td>'+cellVal(cm,'text-end')+'</tr>';
        h+='<tr class="lf-grand-total"><td class="lf-row-num">11</td><td><strong>IS DÛ (max IS calc, CM)</strong></td>'+cellVal(Math.max(isCalc,cm),'text-end fw-bold')+'</tr>';
        h+='<tr><td class="lf-row-num">12</td><td>Acomptes versés</td>'; h+=cell('BA_ETA'); h+='</tr>';
        h+='<tr class="lf-grand-total"><td class="lf-row-num">13</td><td><strong>RELIQUAT À PAYER / EXCÉDENT</strong></td>'+cellVal(Math.max(isCalc,cm)-Formula.compute('BA_ETA'),'text-end fw-bold')+'</tr>';
        h+='</tbody></table></div>';

        el.innerHTML = h;
    }
    /* ─── RENDER: CONTRÔLES ─── */
    function renderControles(){
        var el = document.getElementById('pane-controles');
        if(!el) return;
        Formula.clearCache();

        var checks = [];
        var totalActifBrut = Formula.compute('BA_TOTAL');
        var totalActifAmort = Formula.compute('BA_TAMM');
        var totalActifNet = totalActifBrut - totalActifAmort;
        var totalPassif = Formula.compute('BP_TOTAL');
        var ca = Formula.compute('CPC_CA');
        var rne = Formula.compute('CPC_RNE');
        var tpeProd = Formula.compute('CPC_TPE');
        var tceChrg = Formula.compute('CPC_TCE');
        var balD = Balance.totalSD();
        var balC = Balance.totalSC();

        checks.push({name:'Équilibre Bilan (Actif Net = Passif)', ok:Math.abs(totalActifNet-totalPassif)<1,
            detail:'Actif Net: '+fmt(totalActifNet)+' | Passif: '+fmt(totalPassif)+' | Écart: '+fmt(totalActifNet-totalPassif)});
        checks.push({name:'Équilibre Balance (SD = SC)', ok:Math.abs(balD-balC)<1,
            detail:'Soldes Débit: '+fmt(balD)+' | Soldes Crédit: '+fmt(balC)+' | Écart: '+fmt(balD-balC)});
        checks.push({name:'Résultat CPC = Résultat Bilan', ok:Math.abs(rne-Formula.compute('BP_RNE'))<1,
            detail:'CPC RNE: '+fmt(rne)+' | Bilan RNE: '+fmt(Formula.compute('BP_RNE'))});
        checks.push({name:'Journal équilibré (Débit = Crédit)', ok:Math.abs(Journal.totalDebit()-Journal.totalCredit())<1,
            detail:'Débit: '+fmt(Journal.totalDebit())+' | Crédit: '+fmt(Journal.totalCredit())});
        checks.push({name:'Chiffre d\'affaires positif', ok:ca>0,
            detail:'CA: '+fmt(ca)});
        checks.push({name:'Total Produits ≥ Total Charges', ok:tpeProd>=tceChrg*0.5,
            detail:'Produits: '+fmt(tpeProd)+' | Charges: '+fmt(tceChrg)});

        var passed = checks.filter(function(c){return c.ok;}).length;

        var h = '<h6 class="lf-panel-title">CONTRÔLES DE COHÉRENCE</h6>';
        h+='<div class="lf-kpi-bar">';
        h+='<div class="lf-kpi '+(passed===checks.length?'lf-kpi-ok':'lf-kpi-err')+'"><span class="lf-kpi-label">Résultat</span><span class="lf-kpi-value">'+passed+'/'+checks.length+' OK</span></div>';
        h+='</div>';

        h+='<div class="lf-table-wrap"><table class="lf-table">';
        h+='<thead><tr><th class="lf-row-num">#</th><th>Contrôle</th><th class="text-center">Statut</th><th>Détail</th></tr></thead>';
        h+='<tbody>';
        checks.forEach(function(c,i){
            h+='<tr class="'+(c.ok?'':'table-danger')+'">';
            h+='<td class="lf-row-num">'+(i+1)+'</td>';
            h+='<td>'+_escHtml(c.name)+'</td>';
            h+='<td class="text-center">'+(c.ok?'<span class="badge bg-success"><i class="bi bi-check-circle"></i> OK</span>':'<span class="badge bg-danger"><i class="bi bi-x-circle"></i> ERREUR</span>')+'</td>';
            h+='<td><small>'+_escHtml(c.detail)+'</small></td>';
            h+='</tr>';
        });
        h+='</tbody></table></div>';

        /* Custom formulas summary */
        var customs = Storage.getClientFormulas();
        var cKeys = Object.keys(customs);
        if(cKeys.length){
            h+='<h6 class="lf-panel-title mt-3">FORMULES PERSONNALISÉES ('+cKeys.length+')</h6>';
            h+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
            h+='<thead><tr><th>Code</th><th>Type</th><th>Comptes</th><th class="text-center">Action</th></tr></thead>';
            h+='<tbody>';
            cKeys.forEach(function(k){
                var f=customs[k];
                h+='<tr><td class="lf-acct-code">'+k+'</td><td>'+f.type+'</td>';
                h+='<td>'+(f.accounts||[]).join(', ')+'</td>';
                h+='<td class="text-center"><button class="btn btn-sm btn-outline-danger" onclick="LiasseFiscale.UI.resetFormula(\''+k+'\')"><i class="bi bi-trash"></i></button></td></tr>';
            });
            h+='</tbody></table></div>';
        }

        /* Overrides summary */
        var overrides = Storage.get('overrides')||{};
        var oKeys = Object.keys(overrides);
        if(oKeys.length){
            h+='<h6 class="lf-panel-title mt-3">VALEURS MANUELLES ('+oKeys.length+')</h6>';
            h+='<div class="lf-table-wrap"><table class="lf-table lf-table-sm">';
            h+='<thead><tr><th>Code</th><th class="text-end">Valeur</th><th class="text-center">Action</th></tr></thead>';
            h+='<tbody>';
            oKeys.forEach(function(k){
                h+='<tr><td class="lf-acct-code">'+k+'</td><td class="text-end">'+fmt(overrides[k])+'</td>';
                h+='<td class="text-center"><button class="btn btn-sm btn-outline-danger" onclick="LiasseFiscale.UI.resetOverride(\''+k+'\')"><i class="bi bi-trash"></i></button></td></tr>';
            });
            h+='</tbody></table></div>';
        }

        el.innerHTML = h;
    }

    /* ─── NAVIGATION & FILTERS ─── */
    function filterJournal(val){renderJournal(val);}
    function filterGL(val){renderGrandLivre(val);}

    function resetFormula(code){
        Storage.removeFormula(code);
        Formula.clearCache();
        renderControles();
    }
    function resetOverride(code){
        Storage.removeOverride(code);
        Formula.clearCache();
        renderControles();
    }

    /* ─── PANE ROUTER ─── */
    function onPaneShown(target){
        _currentPane = target;
        switch(target){
            case 'bilan':         renderBilan(); break;
            case 'cpc':           renderCPC(); break;
            case 'esg':           renderESG(); break;
            case 'tf':            renderTF(); break;
            case 'etic':          renderETIC(); break;
            case 'detail-cpc':    renderDetailCPC(); break;
            case 'passage':       renderPassage(); break;
            case 'affectation':   renderAffectation(); break;
            case 'tva':           renderTVADetail(); break;
            case 'immob':         renderImmob(); break;
            case 'amort':         renderAmort(); break;
            case 'provisions':    renderProvisions(); break;
            case 'creances':      renderCreances(); break;
            case '9421':          render9421(); break;
            case 'controles':     renderControles(); break;
            case 'donnees-is':    break; /* Handled by Razor form */
        }
    }

    /* ─── EXPORT ─── */
    function exportData(){
        Formula.clearCache();
        var data = {
            client: _clientId,
            exercice: _exercice,
            declaration: _declId,
            date: new Date().toISOString(),
            balance: Balance.getAll(),
            bilan: {
                totalActifBrut: Formula.compute('BA_TOTAL'),
                totalAmort: Formula.compute('BA_TAMM'),
                totalActifNet: Formula.compute('BA_TOTAL') - Formula.compute('BA_TAMM'),
                totalPassif: Formula.compute('BP_TOTAL')
            },
            cpc: {
                ca: Formula.compute('CPC_CA'),
                rex: Formula.compute('CPC_REX'),
                rfi: Formula.compute('CPC_RFI'),
                rco: Formula.compute('CPC_RCO'),
                rnc: Formula.compute('CPC_RNC'),
                rne: Formula.compute('CPC_RNE')
            },
            customFormulas: Storage.getClientFormulas(),
            overrides: Storage.get('overrides') || {}
        };
        var blob = new Blob([JSON.stringify(data, null, 2)], {type:'application/json'});
        var a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'liasse-fiscale-'+_exercice+'.json';
        a.click();
    }

    /* ─── INIT ─── */
    function init(clientId, exercice, declId, balanceRows){
        _clientId = clientId;
        _exercice = exercice;
        _declId = declId;
        _usingDemo = false;

        Storage.setContext(declId, clientId, exercice);
        var rows = balanceRows || [];
        if(rows.length){
            Journal.clear();
            Balance.loadFromApi(rows);
            _usingDemo = false;
        } else {
            Journal.init(exercice);
            Ledger.build();
            Balance.build();
            _usingDemo = true;
        }
        Formula.clearCache();

        console.log('[LiasseFiscale v5] Init — client:'+clientId+' ex:'+exercice+' demo:'+_usingDemo+' balance:'+Balance.getAll().length);
    }

    /* ─── PUBLIC API ─── */
    return {
        init:init,
        onPaneShown:onPaneShown,
        openConfig:openConfig,
        saveConfig:saveConfig,
        resetConfig:resetConfig,
        closeConfig:closeConfig,
        selectCell:selectCell,
        focusFormulaBar:focusFormulaBar,
        applyFormulaBar:applyFormulaBar,
        cancelFormulaBar:cancelFormulaBar,
        bindFormulaBar:bindFormulaBar,
        toggleAccountPanel:toggleAccountPanel,
        isUsingDemo:function(){return _usingDemo;},
        filterJournal:filterJournal,
        filterGL:filterGL,
        resetFormula:resetFormula,
        resetOverride:resetOverride,
        exportData:exportData,
        getServerSnapshot:function(){
            try{
                var snap={ overrides:Storage.get('overrides')||{}, journal:Storage.get('journal')||[], formulas:Storage.getClientFormulas()||{} };
                return snap;
            }catch(e){ return {}; }
        }
    };
})(); /* end UI */

return {
    PCM:PCM, Storage:Storage, Journal:Journal, Ledger:Ledger, Balance:Balance,
    Formula:Formula, UI:UI
};
})(); /* end LiasseFiscale IIFE */