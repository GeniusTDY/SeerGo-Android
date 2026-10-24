#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
重启号 地图精灵/勇者之塔/试炼之塔 数据 → HTML 生成器
用法: python generate.py
输出: index.html（独立文件，双击即可在浏览器打开）
"""

import os, json, html as h, xml.etree.ElementTree as ET
from datetime import datetime
from collections import OrderedDict

BASE = os.path.dirname(os.path.abspath(__file__))


# ═══════════════════════════════════════════════
# 数据加载
# ═══════════════════════════════════════════════

def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def load_skills():
    data = load_json(os.path.join(BASE, "skills", "skill.json"))
    return {str(m["ID"]): {"n": m.get("Name",""), "p": m.get("Power",""), "a": m.get("Accuracy","")}
            for m in data.get("moves", [])}

def load_buffs():
    data = load_json(os.path.join(BASE, "buffs", "BuffConfig.json"))
    return {str(b["id"]): {"n": b.get("name",""), "d": b.get("description","")}
            for b in data.get("buffs", [])}

def load_monsters():
    """加载精灵数据，返回 {id → DefName}"""
    path = os.path.join(BASE, "mons", "monsters.json")
    data = load_json(path)
    return {m["ID"]: m.get("DefName", "") for m in data.get("Monsters", [])}

def load_items():
    """加载道具数据，返回 {id → {name, description}}"""
    path = os.path.join(BASE, "items", "item.xml")
    tree = ET.parse(path)
    items = {}
    for el in tree.getroot().findall("item"):
        iid = el.get("id", "")
        items[iid] = {
            "name": el.get("name", ""),
            "desc": el.get("description", "")
        }
    return items

def load_new_se():
    """加载融合特性数据，返回 {Idx → {Desc, Intro}}"""
    path = os.path.join(BASE, "fusion", "new_se.json")
    data = load_json(path)
    return {se["Idx"]: {"desc": se.get("Desc",""), "intro": se.get("Intro","")}
            for se in data.get("NewSe", {}).get("NewSeIdx", [])}

def t(el, tag):
    c = el.find(tag)
    return c.text.strip() if c is not None and c.text else ""


# ═══════════════════════════════════════════════
# 地图 XML 解析
# ═══════════════════════════════════════════════

def parse_map(filepath):
    try:
        root = ET.parse(filepath).getroot()
    except ET.ParseError:
        return None
    mid, mname = root.get("id", ""), root.get("name", "")
    entities = root.find("entities")
    if entities is None:
        return {"id": mid, "name": mname, "npcs": []}

    npcs = []
    for npc_el in entities.findall("npc"):
        battle = npc_el.find("battle")
        if battle is None: continue
        na = (npc_el.get("name") or "").strip()
        nd = t(npc_el, "description")
        rate = (npc_el.get("rate") or "").strip()
        # 解析战斗奖励
        raw_rewards = []
        eh = npc_el.find("eventHandler")
        if eh is not None:
            for btn in eh.findall("button"):
                if btn.get("type") == "click":
                    act = btn.find("action")
                    if act is not None and (act.text or "").strip() == "battle":
                        for rw in btn.findall("reward"):
                            raw_rewards.append({
                                "id": rw.get("id", ""),
                                "num": rw.get("num", ""),
                                "rate": rw.get("rate", "")
                            })
        dialog = npc_el.find("dialog")
        branches = battle.findall("branch")
        if not branches: continue

        dbn = {}
        if dialog is not None:
            for db in dialog.findall("branch"):
                bid = db.get("id", "default")
                ns = []
                for f in (db.find("functionHandler") or []):
                    if hasattr(f, "findall"):
                        ns = []
                    else:
                        for fu in f.findall("func") if f is not None else []:
                            if t(fu, "action") == "battle":
                                d = t(fu, "description")
                                if d: ns.append(d)
                if ns: dbn[bid] = "/".join(ns)

        for branch in branches:
            enemy = branch.find("enemy")
            if enemy is None: continue
            is_boss = (enemy.get("type") or "").strip().upper() == "BOSS"
            pet = enemy.find("pet")
            if pet is None: continue
            pid = pet.get("id", "")
            plv = pet.get("level", "")
            bid = branch.get("id", "default")

            if dialog is not None and bid in dbn:
                name = dbn[bid]
            elif is_boss:
                name = nd or na
            else:
                name = na or nd
            if not name: name = f"未知(pet:{pid})"

            status = OrderedDict()
            se = pet.find("status")
            if se is not None:
                for c in se:
                    status[c.tag] = (c.text or "").strip()

            buff_ids = []
            be = pet.find("initBuff")
            if be is not None:
                for b in be.findall("buff"):
                    v = b.get("id", "")
                    if v: buff_ids.append(v)

            st = t(pet, "loopSkill")
            skill_ids = [s.strip() for s in st.split(",") if s.strip()] if st else []

            npcs.append({
                "name": name, "npc_name": na, "boss": is_boss, "rare": bool(rate),
                "rate": rate, "pet_id": pid, "level": plv,
                "status": status, "buff_ids": buff_ids, "skill_ids": skill_ids,
                "raw_rewards": raw_rewards,
            })
    return {"id": mid, "name": mname, "npcs": npcs}


# ═══════════════════════════════════════════════
# 塔数据解析
# ═══════════════════════════════════════════════

def process_tower(filepath, skills, monsters, items):
    """处理勇者之塔/试炼之塔数据，返回处理后的楼层列表"""
    data = load_json(filepath)
    floors_data = data.get("floors", [])
    rewards_data = data.get("rewards", {})

    floors = []
    for f in floors_data:
        fn = f["floor"]
        reward = rewards_data.get(str(fn), {})
        mons_list = []
        for m in f.get("monsters", []):
            mid = m["id"]
            mname = monsters.get(mid, f"未知精灵({mid})")
            mskills = []
            for sid in m.get("skills", []):
                sk = skills.get(str(sid), {})
                mskills.append({
                    "id": str(sid),
                    "name": sk.get("n") or f"未知技能({sid})",
                    "power": sk.get("p", ""),
                    "acc": sk.get("a", "")
                })
            mons_list.append({
                "id": mid, "name": mname,
                "level": m.get("level", 0),
                "skills": mskills
            })

        # 奖励处理
        ritems = []
        for it in reward.get("items", []):
            iid = it.get("itemId", "")
            info = items.get(iid, {})
            ritems.append({
                "id": iid,
                "name": info.get("name") or f"道具{iid}",
                "desc": info.get("desc", ""),
                "count": it.get("count", 0)
            })

        pet_reward = None
        pr_id = reward.get("petRewardId", 0)
        if pr_id:
            pet_reward = {"id": pr_id, "name": monsters.get(pr_id, f"精灵{pr_id}")}

        floors.append({
            "floor": fn,
            "mons": mons_list,
            "coins": reward.get("coins", 0),
            "items": ritems,
            "goldBeans": reward.get("goldBeans", 0),
            "milestoneGoldBeans": reward.get("milestoneGoldBeans", 0),
            "petReward": pet_reward,
            "expPool": reward.get("expPool", 0),
        })

    return floors


# ═══════════════════════════════════════════════
# 融合数据解析
# ═══════════════════════════════════════════════

def parse_fusion_formula(items, new_se):
    """解析 FusionFormula.xml，返回材料配方列表"""
    path = os.path.join(BASE, "fusion", "FusionFormula.xml")
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError:
        return []

    formulas = []
    POS_LABELS = ["上", "右", "下", "左"]  # order 对应 上右下左
    for g in root.findall("group"):
        order_str = g.get("order", "")
        orders = [s.strip() for s in order_str.split(",") if s.strip()]
        ids_str = g.findtext("ids", "")
        feat_ids = [s.strip() for s in ids_str.split(",") if s.strip()]

        # 解析材料
        materials = []
        for i, oid in enumerate(orders):
            info = items.get(oid, {})
            materials.append({
                "pos": POS_LABELS[i] if i < len(POS_LABELS) else "?",
                "id": oid,
                "name": info.get("name") or f"物品{oid}",
                "desc": info.get("desc", "")
            })

        # 解析特性
        features = []
        for fid in feat_ids:
            try: fi = int(fid)
            except: fi = 0
            se = new_se.get(fi, {})
            features.append({
                "id": fid,
                "name": se.get("desc") or f"特性{fid}",
                "intro": se.get("intro", "")
            })

        formulas.append({"materials": materials, "features": features})
    return formulas


def parse_fusion_recipes(monsters, items):
    """解析 FusionRecipes.xml，返回精灵融合配方列表"""
    path = os.path.join(BASE, "fusion", "FusionRecipes.xml")
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError:
        return []

    recipes = []
    for g in root.findall("group"):
        main_id_str = g.findtext("mainId", "").strip()
        mid = int(main_id_str) if main_id_str else 0

        sub_ids = []
        for sub in g.findall("subId"):
            s = (sub.text or "").strip()
            if s: sub_ids.append(int(s))

        results = []
        for r in g.findall("results/result"):
            rid_str = r.get("id", "")
            rid = int(rid_str) if rid_str else 0
            rate = int(r.get("rate", "0"))
            bead_id = r.get("beadid", "")
            bead_info = items.get(bead_id, {})
            results.append({
                "id": rid,
                "name": monsters.get(rid, f"精灵{rid}"),
                "rate": rate,
                "beadId": bead_id,
                "beadName": bead_info.get("name") or f"元神珠{bead_id}",
                "beadDesc": bead_info.get("desc", "")
            })

        main_pet = {"id": mid, "name": monsters.get(mid, f"精灵{mid}")}
        sub_pets = [{"id": sid, "name": monsters.get(sid, f"精灵{sid}")} for sid in sub_ids]

        recipes.append({"main": main_pet, "subs": sub_pets, "results": results})
    return recipes


# ═══════════════════════════════════════════════
# 常量
# ═══════════════════════════════════════════════

STAT_KEYS = ["hp", "attack", "defence", "spAttack", "spDefence", "speed"]
STAT_LABELS = {"hp": "HP", "attack": "攻击", "defence": "防御",
               "spAttack": "特攻", "spDefence": "特防", "speed": "速度"}

CSS = r"""/* ══════ 变量（暗色默认） ══════ */
:root, [data-theme="dark"]{
--bg:#0d1117;--bg2:#161b22;--bg3:#21262d;--bg4:#30363d;
--fg:#e6edf3;--fg2:#8b949e;--fg3:#6e7681;
--ac:#58a6ff;--ac2:#1f6feb;--ac3:#388bfd;
--boss:#f85149;--boss-bg:rgba(248,81,73,.12);
--rare:#d29922;--rare-bg:rgba(210,153,34,.12);
--norm:#3fb950;--norm-bg:rgba(63,185,80,.12);
--sk-bg:rgba(56,139,253,.1);--sk-bd:rgba(56,139,253,.25);
--bf-bg:rgba(188,140,255,.1);--bf-bd:rgba(188,140,255,.25);
--reward-bg:rgba(63,185,80,.08);--reward-bd:rgba(63,185,80,.2);
--gold-fg:#ffd700;--gold-bg:rgba(255,215,0,.1);--gold-bd:rgba(255,215,0,.3);
--sc-fg:#b0b8c4;
--sb-bg:#0d1117;--sb-hover:#21262d;--sb-active:#1f6feb;--sb-fg:#8b949e;--sb-fg-active:#fff;
--br:6px;--r:8px;
}
[data-theme="light"]{
--bg:#ffffff;--bg2:#f6f8fa;--bg3:#eaeef2;--bg4:#d0d7de;
--fg:#1f2328;--fg2:#656d76;--fg3:#8b949e;
--ac:#0969da;--ac2:#0550ae;--ac3:#218bff;
--boss:#cf222e;--boss-bg:rgba(207,34,46,.1);
--rare:#9a6700;--rare-bg:rgba(154,103,0,.1);
--norm:#1a7f37;--norm-bg:rgba(26,127,55,.1);
--sk-bg:rgba(9,105,218,.08);--sk-bd:rgba(9,105,218,.2);
--bf-bg:rgba(130,80,223,.08);--bf-bd:rgba(130,80,223,.2);
--reward-bg:rgba(26,127,55,.08);--reward-bd:rgba(26,127,55,.2);
--gold-fg:#8b6914;--gold-bg:rgba(154,103,0,.08);--gold-bd:rgba(154,103,0,.2);
--sc-fg:#656d76;
--sb-bg:#f6f8fa;--sb-hover:#eaeef2;--sb-active:#0969da;--sb-fg:#656d76;--sb-fg-active:#fff;
}

*{margin:0;padding:0;box-sizing:border-box}
html{font-size:16px;scroll-behavior:smooth}
body{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI","PingFang SC","Microsoft YaHei",sans-serif;background:var(--bg);color:var(--fg);line-height:1.55;-webkit-font-smoothing:antialiased;display:flex;min-height:100vh}

/* ══════ 侧边栏 ══════ */
.sb{position:fixed;left:0;top:0;bottom:0;width:200px;background:var(--sb-bg);border-right:1px solid var(--bg4);z-index:200;display:flex;flex-direction:column;overflow-y:auto;overflow-x:hidden;padding-top:8px}
.sb-logo{padding:12px 16px 16px;font-size:14px;font-weight:700;letter-spacing:.5px;display:flex;align-items:center;gap:8px;border-bottom:1px solid var(--bg4);margin-bottom:8px;flex-shrink:0}
.sb-logo svg{width:20px;height:20px;flex-shrink:0}
.sb-logo em{color:var(--ac);font-style:normal}
.sb-nav{flex:1;display:flex;flex-direction:column;gap:2px;padding:0 8px}
.sb-item{display:flex;align-items:center;gap:8px;padding:9px 12px;border-radius:6px;cursor:pointer;font-size:13px;color:var(--sb-fg);transition:all .15s;border:none;background:transparent;text-align:left;width:100%;font-family:inherit;white-space:nowrap}
.sb-item:hover{background:var(--sb-hover);color:var(--fg)}
.sb-item.active{background:var(--sb-active);color:var(--sb-fg-active);font-weight:600}
.sb-item svg{width:16px;height:16px;flex-shrink:0}
.sb-footer{padding:12px 16px;border-top:1px solid var(--bg4);display:flex;align-items:center;justify-content:space-between;flex-shrink:0}
.sb-ver{font-size:10px;color:var(--fg3);white-space:nowrap}
/* 主题切换 */
.thm-btn{width:32px;height:32px;border-radius:6px;border:1px solid var(--bg4);background:var(--bg3);cursor:pointer;display:flex;align-items:center;justify-content:center;color:var(--fg2);font-size:14px;transition:all .2s;flex-shrink:0}
.thm-btn:hover{border-color:var(--ac);color:var(--ac)}
/* 移动端侧边栏切换按钮 */
.sb-toggle{display:none;position:fixed;top:10px;left:10px;z-index:250;width:36px;height:36px;border-radius:6px;border:1px solid var(--bg4);background:var(--bg2);cursor:pointer;color:var(--fg2);font-size:18px;align-items:center;justify-content:center}

/* ══════ 头部 ══════ */
.top{position:sticky;top:0;z-index:90;background:linear-gradient(135deg,var(--bg) 0%,var(--bg2) 100%);border-bottom:1px solid var(--bg4);padding:0 24px;display:flex;align-items:center;justify-content:space-between;height:52px}
.top-title{font-size:14px;font-weight:600}
.top-stats{display:flex;gap:16px;font-size:12px;color:var(--fg2)}
.top-stats span{display:flex;align-items:center;gap:5px}
.dot{width:8px;height:8px;border-radius:50%}
.dot.b{background:var(--boss)}.dot.r{background:var(--rare)}.dot.n{background:var(--norm)}

/* ══════ 搜索栏 ══════ */
.search{position:sticky;top:52px;z-index:89;background:var(--bg2);border-bottom:1px solid var(--bg4);padding:12px 24px;display:flex;gap:10px;align-items:center;flex-wrap:wrap}
.search input{flex:1;min-width:160px;max-width:360px;padding:8px 14px;background:var(--bg3);border:1px solid var(--bg4);border-radius:20px;color:var(--fg);font-size:13px;outline:none;transition:border-color .2s}
.search input:focus{border-color:var(--ac)}
.search input::placeholder{color:var(--fg3)}
.fb{padding:6px 14px;border:1px solid var(--bg4);border-radius:20px;background:transparent;color:var(--fg2);cursor:pointer;font-size:12px;transition:all .15s;white-space:nowrap}
.fb:hover{border-color:var(--ac);color:var(--ac)}
.fb.on{background:var(--ac2);color:#fff;border-color:var(--ac2)}
.fb.boss.on{background:var(--boss);border-color:var(--boss)}
.fb.rare.on{background:var(--rare);border-color:var(--rare)}
#rc{font-size:12px;color:var(--fg2);margin-left:auto;white-space:nowrap}

/* ══════ 主区域 ══════ */
.wrap{margin-left:200px;flex:1;display:flex;flex-direction:column;min-height:100vh}
.main{max-width:1240px;margin:0 auto;width:100%;padding:16px 24px 80px;flex:1}
.section{display:none}
.section.active{display:block}

/* ══════ 卡片 ══════ */
.card{background:var(--bg2);border:1px solid var(--bg4);border-radius:var(--r);margin-bottom:8px;overflow:hidden;transition:border-color .15s}
.card:hover{border-color:var(--ac3)}
.card.hid{display:none}
.ch{padding:12px 16px;cursor:pointer;display:flex;align-items:center;justify-content:space-between;user-select:none;transition:background .12s}
.ch:hover{background:var(--bg3)}
.ch.on .ci{transform:rotate(180deg)}
.ct{display:flex;align-items:center;gap:10px;flex-wrap:wrap;min-width:0}
.mid{font-family:"SF Mono","Cascadia Code","Consolas",monospace;font-size:12px;color:var(--ac);background:rgba(56,139,253,.1);padding:2px 9px;border-radius:4px;white-space:nowrap}
.mn{font-size:14px;font-weight:600;white-space:nowrap}
.ms{display:flex;gap:5px;flex-wrap:wrap}
.stag{font-size:11px;padding:2px 8px;border-radius:10px;font-weight:500;white-space:nowrap}
.stag.boss{background:var(--boss-bg);color:var(--boss)}
.stag.rare{background:var(--rare-bg);color:var(--rare)}
.stag.normal{background:var(--norm-bg);color:var(--norm)}
.ci{font-size:11px;color:var(--fg3);transition:transform .2s;flex-shrink:0}
.cb{display:none;padding:0 16px 16px;overflow-x:auto;-webkit-overflow-scrolling:touch}
.cb.open{display:block}

/* ══════ 表格 ══════ */
table{width:100%;border-collapse:collapse;font-size:13px;min-width:600px}
thead th{position:sticky;top:0;text-align:left;padding:9px 10px;background:var(--bg3);font-weight:600;font-size:11px;text-transform:uppercase;letter-spacing:.4px;color:var(--fg2);border-bottom:2px solid var(--bg4);white-space:nowrap}
td{padding:8px 10px;border-bottom:1px solid var(--bg4);vertical-align:top}
tbody tr:hover{background:rgba(255,255,255,.03)}
tbody tr.hid{display:none}
tr.boss{border-left:3px solid var(--boss)}
tr.rare{border-left:3px solid var(--rare)}
tr.br{border-left:3px solid var(--boss)}
.c{text-align:center}
.w60{width:60px}.w70{width:70px}.w90{width:90px}.w160{min-width:160px}.w180{min-width:170px}.w280{min-width:260px}
.nc{font-weight:500;white-space:nowrap}
.sc{font-size:11px;color:var(--sc-fg);white-space:nowrap}

/* ══════ 徽章 ══════ */
.badge{display:inline-block;font-size:10px;padding:2px 6px;border-radius:3px;font-weight:700;letter-spacing:.3px;margin-right:4px;vertical-align:middle}
.badge.boss{background:var(--boss);color:#fff}
.badge.rare{background:var(--rare);color:#fff}

/* ══════ 标签 ══════ */
.sk{display:inline-block;font-size:11px;padding:2px 7px;margin:2px 3px 2px 0;background:var(--sk-bg);border:1px solid var(--sk-bd);border-radius:3px;color:var(--ac);white-space:nowrap;cursor:default;transition:background .12s}
.sk:hover{background:rgba(56,139,253,.2)}
.bf{display:inline-block;font-size:11px;padding:2px 7px;margin:2px 3px 2px 0;background:var(--bf-bg);border:1px solid var(--bf-bd);border-radius:3px;color:#bc8cff;white-space:nowrap;cursor:default;transition:background .12s}
.bf:hover{background:rgba(188,140,255,.2)}

.muted{color:var(--fg3);font-style:italic}

/* ══════ 奖励展示 ══════ */
.reward-box{display:flex;flex-wrap:wrap;gap:6px;margin-top:4px}
.reward-tag{display:inline-flex;align-items:center;gap:4px;font-size:11px;padding:3px 8px;border-radius:4px;white-space:nowrap}
.reward-tag.coin{background:var(--gold-bg);border:1px solid var(--gold-bd);color:#d29922}
.reward-tag.gold{background:var(--gold-bg);border:1px solid var(--gold-bd);color:var(--gold-fg)}
.reward-tag.item{background:var(--reward-bg);border:1px solid var(--reward-bd);color:var(--norm)}
.reward-tag.pet{background:rgba(56,139,253,.1);border:1px solid rgba(56,139,253,.25);color:var(--ac)}
.reward-tag.exp{background:rgba(188,140,255,.1);border:1px solid rgba(188,140,255,.25);color:#bc8cff}
.reward-tag .rw-count{font-weight:600;opacity:.85}
.reward-inline{display:inline-flex;align-items:center;gap:2px;font-size:10px;padding:1px 5px;border-radius:3px;background:var(--reward-bg);border:1px solid var(--reward-bd);color:var(--norm);margin:1px 2px;vertical-align:middle;white-space:nowrap}
.disguised{display:inline-block;font-size:10px;font-weight:400;color:var(--ac);background:rgba(56,139,253,.1);padding:1px 5px;border-radius:3px;margin-left:2px;white-space:nowrap;vertical-align:middle}

/* ══════ 塔楼层卡片 ══════ */
.tower-card .ch .mn{font-weight:700}
.tower-card .floor-lv{font-size:11px;color:var(--fg2);margin-left:4px}
.tower-table{min-width:650px}
.tower-table .pet-cell{white-space:nowrap}

/* ══════ 融合材料 ══════ */
.fuse-grid{display:grid;grid-template-columns:1fr 1fr;gap:6px;margin-bottom:10px;max-width:360px}
.fuse-slot{display:flex;align-items:center;gap:6px;padding:8px 10px;background:var(--bg3);border-radius:var(--br);border:1px solid var(--bg4);font-size:12px}
.fuse-slot .pos-label{font-size:10px;font-weight:700;color:var(--ac);background:rgba(56,139,253,.15);padding:1px 6px;border-radius:4px;flex-shrink:0}
.fuse-slot .mat-name{font-weight:500;flex:1}
.fuse-slot .mat-id{font-size:10px;color:var(--fg3);flex-shrink:0}
.fuse-se{display:flex;flex-wrap:wrap;gap:6px;margin-top:8px}
.fuse-se-tag{display:inline-flex;align-items:center;gap:4px;font-size:11px;padding:3px 8px;border-radius:4px;background:rgba(188,140,255,.1);border:1px solid rgba(188,140,255,.25);color:#bc8cff;white-space:nowrap}
.fuse-se-tag .se-pct{font-size:10px;opacity:.7}
.fuse-recipe{margin-bottom:8px}
.fuse-recipe .flow{display:flex;align-items:center;gap:8px;flex-wrap:wrap;padding:8px 0}
.fuse-recipe .pet-tag{display:inline-flex;align-items:center;gap:4px;padding:4px 10px;background:var(--bg3);border:1px solid var(--bg4);border-radius:6px;font-size:12px;font-weight:500}
.fuse-recipe .pet-tag .mt-id{font-size:10px;color:var(--fg3)}
.fuse-recipe .arrow{color:var(--ac);font-weight:700;font-size:16px}
.fuse-recipe .sub-list{display:inline-flex;align-items:center;gap:4px;flex-wrap:wrap}
.fuse-recipe .sub-conn{font-size:11px;color:var(--fg3)}
.fuse-result{display:flex;align-items:center;gap:6px;padding:6px 10px;background:rgba(56,139,253,.08);border:1px solid rgba(56,139,253,.2);border-radius:6px;font-size:12px;font-weight:600;color:var(--ac)}
.fuse-result .rate{font-size:10px;color:var(--fg2)}
.fuse-result .bead{font-size:10px;color:var(--norm);margin-left:2px}
.fuse-recipes-section{margin-top:24px}
.fuse-recipes-section h3{font-size:14px;font-weight:600;margin-bottom:10px;padding:0 4px;border-left:3px solid var(--ac);padding-left:12px}

/* ══════ Footer ══════ */
.ft{text-align:center;padding:20px;color:var(--fg3);font-size:11px;border-top:1px solid var(--bg4)}

/* ══════ 回到顶部 ══════ */
.btt{position:fixed;bottom:28px;right:28px;width:38px;height:38px;border-radius:50%;background:var(--bg3);border:1px solid var(--bg4);color:var(--fg2);cursor:pointer;font-size:16px;display:none;z-index:180;transition:all .2s}
.btt.show{display:flex;align-items:center;justify-content:center}
.btt:hover{background:var(--ac2);color:#fff;border-color:var(--ac2)}

/* ══════ 手机适配 ══════ */
@media(max-width:900px){
  .sb{transform:translateX(-100%);transition:transform .25s;pointer-events:none}
  .sb.open{transform:translateX(0);pointer-events:auto}
  .sb-toggle{display:flex}
  .wrap{margin-left:0}
  .top{padding-left:56px}
  .search{padding-left:56px}
}
@media(max-width:768px){
  html{font-size:15px}
  .top{padding:0 14px 0 52px;height:46px}
  .top-stats{gap:10px;font-size:11px}
  .top-stats span:last-child{display:none}
  .search{padding:10px 14px 10px 52px;top:46px;gap:6px}
  .search input{font-size:12px;padding:7px 12px}
  .fb{font-size:11px;padding:5px 10px}
  .main{padding:8px 8px 60px}
  .card{border-radius:6px;margin-bottom:6px}
  .ch{padding:10px 12px}
  .mn{font-size:13px}
  .mid{font-size:11px}
  .ci{font-size:10px}
  .cb{padding:0 8px 12px}
  table{font-size:11px;min-width:550px}
  thead th{font-size:10px;padding:7px 6px}
  td{padding:6px}
  .sk,.bf{font-size:10px;padding:1px 5px}
  .badge{font-size:9px;padding:1px 5px}
  .sc{font-size:10px}
  .btt{bottom:16px;right:16px;width:34px;height:34px;font-size:14px}
  .ms{gap:3px}
  .stag{font-size:10px;padding:1px 6px}
  .reward-tag{font-size:10px;padding:2px 6px}
}
@media(max-width:480px){
  html{font-size:14px}
  .top-stats{display:none}
  .search{padding:8px 10px 8px 48px}
  .search input{max-width:200px}
  .fb{padding:4px 8px;font-size:10px}
}
"""

# SVG icons
ICON_MAP = '<svg viewBox="0 0 24 24" fill="none"><rect x="3" y="3" width="18" height="18" rx="2" stroke="currentColor" stroke-width="1.5"/><path d="M3 9h18M9 3v18" stroke="currentColor" stroke-width="1"/></svg>'
ICON_FUSE = '<svg viewBox="0 0 24 24" fill="none"><circle cx="9" cy="7" r="3" stroke="currentColor" stroke-width="1.5"/><circle cx="15" cy="17" r="3" stroke="currentColor" stroke-width="1.5"/><path d="M11 9l3 5" stroke="currentColor" stroke-width="1.5"/></svg>'
ICON_BRAVE = '<svg viewBox="0 0 24 24" fill="none"><path d="M12 2l3 7h7l-5.5 4 2 7L12 16l-6.5 4 2-7L2 9h7z" stroke="currentColor" stroke-width="1.5"/></svg>'
ICON_TRIAL = '<svg viewBox="0 0 24 24" fill="none"><rect x="3" y="2" width="18" height="20" rx="3" stroke="currentColor" stroke-width="1.5"/><line x1="12" y1="6" x2="12" y2="12" stroke="currentColor" stroke-width="1.5"/><circle cx="12" cy="16" r="1" fill="currentColor"/></svg>'
ICON_SUN = '<svg viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="5" stroke="currentColor" stroke-width="1.5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" stroke="currentColor" stroke-width="1.5"/></svg>'
ICON_MOON = '<svg viewBox="0 0 24 24" fill="none"><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z" stroke="currentColor" stroke-width="1.5"/></svg>'


# ═══════════════════════════════════════════════
# HTML 生成
# ═══════════════════════════════════════════════

def gen_html(maps, skills, buffs, brave_floors, trial_floors, formulas, recipes, monsters, items, gen_time):
    now = gen_time

    # 地图统计
    all_npcs = []
    for m in maps:
        for n in m["npcs"]:
            n["_map_id"] = m["id"]
            n["_map_name"] = m["name"]
            n["_skills"] = []
            for sid in n["skill_ids"]:
                sk = skills.get(sid, {})
                n["_skills"].append({"id": sid, "name": sk.get("n") or f"未知技能({sid})",
                                      "power": sk.get("p",""), "acc": sk.get("a","")})
            n["_buffs"] = []
            for bid in n["buff_ids"]:
                bf = buffs.get(bid, {})
                n["_buffs"].append({"id": bid, "name": bf.get("n") or f"未知Buff({bid})",
                                     "desc": bf.get("d","")})
            parts = []
            for k in STAT_KEYS:
                if k in n["status"]:
                    parts.append(f"{STAT_LABELS[k]}:{n['status'][k]}")
            for k, v in n["status"].items():
                if k not in STAT_KEYS:
                    parts.append(f"{k}:{v}")
            n["_status_str"] = " | ".join(parts)
            # 尼尔检测: NPC显示名 ≠ 实际精灵名
            pid_int = int(n["pet_id"]) if n["pet_id"].lstrip("-").isdigit() else 0
            mn_name = monsters.get(pid_int, "")
            n["_is_disguised"] = bool(mn_name and mn_name != n["name"] and mn_name != n.get("npc_name",""))
            n["_monster_name"] = mn_name
            # 奖励解析
            n["_rewards"] = []
            for rw in n.get("raw_rewards", []):
                info = items.get(rw["id"], {})
                n["_rewards"].append({
                    "id": rw["id"], "num": rw["num"], "rate": rw["rate"],
                    "name": info.get("name") or f"道具{rw['id']}",
                    "desc": info.get("desc", "")
                })
            all_npcs.append(n)

    tb = sum(1 for x in all_npcs if x["boss"])
    tr = sum(1 for x in all_npcs if x["rare"])
    tn = sum(1 for x in all_npcs if not x["boss"] and not x["rare"])
    total = len(all_npcs)

    # ── 地图 section ──
    map_parts = []
    for m in maps:
        bc = sum(1 for n in m["npcs"] if n["boss"])
        rc = sum(1 for n in m["npcs"] if n["rare"])
        nc = sum(1 for n in m["npcs"] if not n["boss"] and not n["rare"])

        stags = ""
        if bc: stags += f'<span class="stag boss">BOSS×{bc}</span>'
        if rc: stags += f'<span class="stag rare">稀有×{rc}</span>'
        if nc: stags += f'<span class="stag normal">普通×{nc}</span>'

        rows = ""
        for n in m["npcs"]:
            badges = ""
            if n["boss"]: badges += '<span class="badge boss">BOSS</span>'
            if n["rare"]:
                rtxt = f"稀有({n['rate']}%)" if n["rate"] else "稀有"
                badges += f'<span class="badge rare">{h.escape(rtxt)}</span>'
            cls = "br" if n["boss"] and n["rare"] else ("boss" if n["boss"] else ("rare" if n["rare"] else ""))

            sk_html = " ".join(
                f'<span class="sk" title="ID:{h.escape(s["id"])}'
                + (f' 威力{s["power"]}' if s["power"] else "")
                + (f' 命中{s["acc"]}' if s["acc"] else "") + '">'
                + h.escape(s["name"]) + '</span>'
                for s in n["_skills"]
            ) or '<span class="muted">-</span>'

            bf_html = " ".join(
                f'<span class="bf" title="ID:{h.escape(b["id"])}'
                + (f' - {h.escape(b["desc"])}' if b["desc"] else "") + '">'
                + h.escape(b["name"]) + '</span>'
                for b in n["_buffs"]
            ) or '<span class="muted">-</span>'

            # 尼尔标记
            name_disp = h.escape(n["name"])
            if n.get("_is_disguised"):
                name_disp += f' <span class="disguised">→ {h.escape(n["_monster_name"])}</span>'

            # 战斗奖励标签
            reward_tags = ""
            if n["_rewards"]:
                rtags = []
                for rw in n["_rewards"]:
                    title = h.escape(rw["desc"]) if rw["desc"] else ""
                    rtags.append(
                        f'<span class="reward-inline" title="{title}">'
                        f'{h.escape(rw["name"])} ×{rw["num"]} ({rw["rate"]}%)'
                        f'</span>'
                    )
                reward_tags = " " + " ".join(rtags)

            rows += (
                f'<tr class="{cls}">'
                f'<td class="nc">{badges} {name_disp}{reward_tags}</td>'
                f'<td class="c">Lv.{h.escape(n["level"])}</td>'
                f'<td class="c">{h.escape(n["pet_id"])}</td>'
                f'<td>{sk_html}</td>'
                f'<td>{bf_html}</td>'
                f'<td class="sc">{h.escape(n.get("_status_str","")) or "-"}</td>'
                f'</tr>'
            )

        if m["npcs"]:
            tbl = (
                '<table><thead><tr>'
                '<th>精灵名称</th><th class="c w60">等级</th><th class="c w70">ID</th>'
                '<th class="w280">技能</th><th class="w180">Buff</th><th class="w160">六维</th>'
                '</tr></thead><tbody>' + rows + '</tbody></table>'
            )
        else:
            tbl = '<p class="muted c">无战斗NPC</p>'

        map_parts.append(
            f'<div class="card" data-mid="{h.escape(m["id"])}">'
            f'<div class="ch" onclick="tg(this)">'
            f'<div class="ct">'
            f'<span class="mid">{h.escape(m["id"])}</span>'
            f'<span class="mn">{h.escape(m["name"])}</span>'
            f'<span class="ms">{stags}</span>'
            f'</div><span class="ci">&#9660;</span>'
            f'</div><div class="cb">{tbl}</div>'
            f'</div>'
        )

    # ── 塔 section ──
    def gen_tower_html(floors, tower_tag):
        """生成塔楼层的HTML卡片"""
        parts = []
        for f in floors:
            fn = f["floor"]
            mons = f["mons"]

            # 精灵表格
            rows = ""
            for m in mons:
                sks = " ".join(
                    f'<span class="sk" title="ID:{h.escape(s["id"])}'
                    + (f' 威力{s["power"]}' if s["power"] else "")
                    + (f' 命中{s["acc"]}' if s["acc"] else "") + '">'
                    + h.escape(s["name"]) + '</span>'
                    for s in m["skills"]
                ) or '<span class="muted">-</span>'

                rows += (
                    f'<tr>'
                    f'<td class="nc pet-cell">{h.escape(m["name"])} <span class="mid">{m["id"]}</span></td>'
                    f'<td class="c">Lv.{m["level"]}</td>'
                    f'<td class="w280">{sks}</td>'
                    f'</tr>'
                )

            # 奖励标签
            reward_tags = []
            if f["coins"]:
                reward_tags.append(f'<span class="reward-tag coin">💰赛尔豆 <span class="rw-count">x{f["coins"]}</span></span>')
            if f["goldBeans"]:
                reward_tags.append(f'<span class="reward-tag gold">🪙金豆 <span class="rw-count">x{f["goldBeans"]}</span></span>')
            if f["milestoneGoldBeans"]:
                reward_tags.append(f'<span class="reward-tag gold">🏆里程金豆 <span class="rw-count">x{f["milestoneGoldBeans"]}</span></span>')
            for it in f["items"]:
                title = h.escape(it["desc"]) if it["desc"] else ""
                reward_tags.append(
                    f'<span class="reward-tag item" title="{title}">'
                    f'{h.escape(it["name"])} <span class="rw-count">x{it["count"]}</span></span>'
                )
            if f["petReward"]:
                reward_tags.append(
                    f'<span class="reward-tag pet">🐾精灵奖励: {h.escape(f["petReward"]["name"])} (ID:{f["petReward"]["id"]})</span>'
                )
            if f["expPool"]:
                reward_tags.append(f'<span class="reward-tag exp">✨经验池: {f["expPool"]}</span>')

            reward_html = '<div class="reward-box">' + "".join(reward_tags) + '</div>' if reward_tags else '<span class="muted">无奖励</span>'

            m_count = len(mons)
            tbl = (
                f'<table class="tower-table"><thead><tr>'
                f'<th>精灵</th><th class="c w60">等级</th><th class="w280">技能</th>'
                f'</tr></thead><tbody>{rows}</tbody></table>'
            ) if mons else '<p class="muted c">无数据</p>'

            parts.append(
                f'<div class="card tower-card" data-fn="{fn}" data-tag="{tower_tag}">'
                f'<div class="ch" onclick="tg(this)">'
                f'<div class="ct">'
                f'<span class="mid">第{fn}层</span>'
                f'<span class="mn">{m_count}只精灵</span>'
                f'<span class="floor-lv">当前 Lv.{max(m["level"] for m in mons) if mons else "?"}</span>'
                f'</div><span class="ci">&#9660;</span>'
                f'</div><div class="cb">{tbl}{reward_html}</div>'
                f'</div>'
            )
        return "\n".join(parts)

    brave_html = gen_tower_html(brave_floors, "brave")
    trial_html = gen_tower_html(trial_floors, "trial")

    # 塔统计
    bf_count = len(brave_floors)
    tf_count = len(trial_floors)
    bm_count = sum(len(f["mons"]) for f in brave_floors)
    tm_count = sum(len(f["mons"]) for f in trial_floors)

    # ── 融合 section ──
    fuse_parts = []

    # 材料配方
    for fi, fm in enumerate(formulas):
        # 材料格子
        slots_html = ""
        for m in fm["materials"]:
            title = h.escape(m["desc"]) if m["desc"] else ""
            slots_html += (
                f'<div class="fuse-slot" title="{title}">'
                f'<span class="pos-label">{m["pos"]}</span>'
                f'<span class="mat-name">{h.escape(m["name"])}</span>'
                f'<span class="mat-id">{h.escape(m["id"])}</span>'
                f'</div>'
            )

        # 特性
        se_tags = ""
        if fm["features"]:
            se_items = []
            for fe in fm["features"]:
                ftitle = h.escape(fe["intro"]) if fe["intro"] else ""
                se_items.append(
                    f'<span class="fuse-se-tag" title="{ftitle}">'
                    f'{h.escape(fe["name"])} <span class="se-pct">(1/{len(fm["features"])})</span>'
                    f'</span>'
                )
            se_tags = '<div class="fuse-se">' + "".join(se_items) + '</div>'
        else:
            se_tags = '<span class="muted">无特性</span>'

        fuse_parts.append(
            f'<div class="card fuse-card" data-fid="{fi}">'
            f'<div class="ch" onclick="tg(this)">'
            f'<div class="ct">'
            f'<span class="mid">材料配方 #{fi + 1}</span>'
            f'<span class="mn">{len(fm["materials"])}种材料 → {len(fm["features"])}种特性</span>'
            f'</div><span class="ci">&#9660;</span>'
            f'</div>'
            f'<div class="cb">'
            f'<div class="fuse-grid">{slots_html}</div>'
            f'<div style="font-size:12px;color:var(--fg2);margin-bottom:4px">随机获得以下特性之一：</div>'
            f'{se_tags}'
            f'</div>'
            f'</div>'
        )

    # 精灵融合配方
    fuse_parts.append('<div class="fuse-recipes-section"><h3>精灵融合配方</h3></div>')
    for ri, rp in enumerate(recipes):
        # 结果
        result_parts = []
        for res in rp["results"]:
            result_parts.append(
                f'<div class="fuse-result">'
                f'{h.escape(res["name"])} <span class="mid" style="font-size:10px">{res["id"]}</span>'
                f'<span class="rate">{res["rate"]}%</span>'
                f'<span class="bead" title="{h.escape(res["beadDesc"])}">🧬{h.escape(res["beadName"])}</span>'
                f'</div>'
            )

        # 副宠列表
        sub_list = []
        for sp in rp["subs"]:
            sub_list.append(
                f'<span class="pet-tag">{h.escape(sp["name"])} <span class="mt-id">({sp["id"]})</span></span>'
            )

        fuse_parts.append(
            f'<div class="card fuse-recipe" data-rid="{ri}">'
            f'<div class="ch" onclick="tg(this)">'
            f'<div class="ct">'
            f'<span class="mid">配方 #{ri + 1}</span>'
            f'<span class="mn">{h.escape(rp["main"]["name"])}</span>'
            f'<span class="floor-lv">+ {len(rp["subs"])}种副宠 → {len(rp["results"])}结果</span>'
            f'</div><span class="ci">&#9660;</span>'
            f'</div>'
            f'<div class="cb">'
            f'<div class="flow">'
            f'<span>主宠：</span>'
            f'<span class="pet-tag">{h.escape(rp["main"]["name"])} <span class="mt-id">({rp["main"]["id"]})</span></span>'
            f'<span class="arrow">+</span>'
            f'<span>副宠(任一)：</span>'
            f'<div class="sub-list">{" ".join(sub_list)}</div>'
            f'<span class="arrow">→</span>'
            f'<span>结果：</span>'
            f'{" ".join(result_parts)}'
            f'</div>'
            f'</div>'
            f'</div>'
        )

    fuse_html = "\n".join(fuse_parts)

    return f"""<!DOCTYPE html>
<html lang="zh-CN" data-theme="dark">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width,initial-scale=1.0,maximum-scale=5.0">
<title>重启号 · 数据总览</title>
<style>
{CSS}
</style>
</head>
<body>

<!-- 侧边栏切换按钮（移动端） -->
<button class="sb-toggle" id="sbt" onclick="ts()">☰</button>

<!-- 侧边栏 -->
<aside class="sb" id="sb">
  <div class="sb-logo">
    <svg viewBox="0 0 24 24" fill="none"><rect x="2" y="2" width="20" height="20" rx="5" stroke="var(--ac)" stroke-width="2"/><circle cx="8" cy="9" r="2" fill="var(--ac)"/><circle cx="16" cy="9" r="2" fill="var(--ac)"/><path d="M7 16c2.5-1.5 7.5-1.5 10 0" stroke="var(--ac)" stroke-width="2" stroke-linecap="round"/></svg>
    <em>Seer</em> 数据
  </div>
  <nav class="sb-nav">
    <button class="sb-item active" data-section="maps" onclick="ss('maps')">
      {ICON_MAP} 地图
    </button>
    <button class="sb-item" data-section="fuse" onclick="ss('fuse')">
      {ICON_FUSE} 融合
    </button>
    <button class="sb-item" data-section="brave" onclick="ss('brave')">
      {ICON_BRAVE} 勇者之塔
    </button>
    <button class="sb-item" data-section="trial" onclick="ss('trial')">
      {ICON_TRIAL} 试炼之塔
    </button>
  </nav>
  <div class="sb-footer">
    <span class="sb-ver">v1.0 · {now}</span>
    <button class="thm-btn" id="thm-btn" onclick="tt()" title="切换主题">{ICON_SUN}</button>
  </div>
</aside>

<!-- 主内容区域 -->
<div class="wrap">

  <!-- 地图 section -->
  <section class="section active" id="sec-maps">
    <header class="top">
      <div class="top-title">地图精灵总览</div>
      <div class="top-stats">
        <span><span class="dot b"></span>BOSS {tb}</span>
        <span><span class="dot r"></span>稀有 {tr}</span>
        <span><span class="dot n"></span>普通 {tn}</span>
        <span>地图 {len(maps)}</span>
      </div>
    </header>
    <div class="search" id="search-maps">
      <input id="si-maps" placeholder="搜索地图/精灵/技能..." oninput="fl()">
      <button class="fb on" data-f="all" onclick="sf('all')">全部</button>
      <button class="fb boss" data-f="boss" onclick="sf('boss')">BOSS</button>
      <button class="fb rare" data-f="rare" onclick="sf('rare')">稀有</button>
      <button class="fb" data-f="norm" onclick="sf('norm')">普通</button>
      <span id="rc-maps">共 {total} 个精灵</span>
    </div>
    <div class="main" id="mc-maps">
{''.join(map_parts)}
    </div>
  </section>

  <!-- 融合 section -->
  <section class="section" id="sec-fuse">
    <header class="top">
      <div class="top-title">精灵融合</div>
      <div class="top-stats">
        <span>材料配方 {len(formulas)} 组</span>
        <span>融合配方 {len(recipes)} 个</span>
      </div>
    </header>
    <div class="search" id="search-fuse">
      <input id="si-fuse" placeholder="搜索材料/精灵/特性..." oninput="tff()">
      <span id="rc-fuse">共 {len(formulas) + len(recipes)} 条</span>
    </div>
    <div class="main" id="mc-fuse">
{fuse_html}
    </div>
  </section>

  <!-- 勇者之塔 section -->
  <section class="section" id="sec-brave">
    <header class="top">
      <div class="top-title">勇者之塔</div>
      <div class="top-stats">
        <span>共 {bf_count} 层</span>
        <span>{bm_count} 只精灵</span>
      </div>
    </header>
    <div class="search" id="search-brave">
      <input id="si-brave" placeholder="搜索楼层/精灵/技能..." oninput="tf('brave')">
      <span id="rc-brave">共 {bf_count} 层</span>
    </div>
    <div class="main" id="mc-brave">
{brave_html}
    </div>
  </section>

  <!-- 试炼之塔 section -->
  <section class="section" id="sec-trial">
    <header class="top">
      <div class="top-title">试炼之塔</div>
      <div class="top-stats">
        <span>共 {tf_count} 层</span>
        <span>{tm_count} 只精灵</span>
      </div>
    </header>
    <div class="search" id="search-trial">
      <input id="si-trial" placeholder="搜索楼层/精灵/技能..." oninput="tf('trial')">
      <span id="rc-trial">共 {tf_count} 层</span>
    </div>
    <div class="main" id="mc-trial">
{trial_html}
    </div>
  </section>

  <footer class="ft">数据来源: maps/*.xml · towers/*.json · fusion/*.xml · fusion/*.json · skill.json · monsters.json · item.xml · BuffConfig.json · 生成时间: {now}</footer>
</div>

<button class="btt" id="btt" onclick="scrollTo({{top:0,behavior:'smooth'}})">↑</button>

<script>
// ══════ 主题切换 ══════
(function(){{
  var s='dark';
  try{{s=localStorage.getItem('seer-theme')||'dark'}}catch(e){{}}
  document.documentElement.setAttribute('data-theme',s);
  updateThemeBtn(s);
}})();
function tt(){{
  var d=document.documentElement;
  var t=d.getAttribute('data-theme')==='dark'?'light':'dark';
  d.setAttribute('data-theme',t);
  try{{localStorage.setItem('seer-theme',t)}}catch(e){{}}
  updateThemeBtn(t);
}}
function updateThemeBtn(t){{
  var b=document.getElementById('thm-btn');
  b.innerHTML=t==='dark'?'{ICON_SUN}':'{ICON_MOON}';
}}

// ══════ 侧边栏 / section 切换 ══════
var curSection='maps';
function ss(id){{
  if(curSection===id) return;
  curSection=id;
  document.querySelectorAll('.section').forEach(function(s){{s.classList.remove('active')}});
  document.getElementById('sec-'+id).classList.add('active');
  document.querySelectorAll('.sb-item').forEach(function(b){{b.classList.remove('active')}});
  document.querySelector('[data-section="'+id+'"]').classList.add('active');
  // 移动端关闭侧边栏
  document.getElementById('sb').classList.remove('open');
  scrollTo({{top:0,behavior:'smooth'}});
}}
function ts(){{
  document.getElementById('sb').classList.toggle('open');
}}

// ══════ 卡片展开/折叠 ══════
function tg(h){{h.classList.toggle('on');h.nextElementSibling.classList.toggle('open')}}

// ══════ 地图搜索 ══════
var cf='all';
function sf(t){{
  cf=t;document.querySelectorAll('.fb').forEach(function(b){{b.classList.remove('on')}});
  document.querySelector('[data-f="'+t+'"]').classList.add('on');fl()
}}
function fl(){{
  var q=(document.getElementById('si-maps').value||'').toLowerCase();
  var vn=0,vm=0;
  document.querySelectorAll('#sec-maps .card').forEach(function(c){{
    var hv=false;
    c.querySelectorAll('tbody tr').forEach(function(r){{
      var s=r.textContent.toLowerCase().includes(q);
      if(cf==='boss')s=s&&r.classList.contains('boss');
      else if(cf==='rare')s=s&&r.classList.contains('rare')&&!r.classList.contains('boss');
      else if(cf==='norm')s=s&&!r.classList.contains('boss')&&!r.classList.contains('rare');
      r.classList.toggle('hid',!s);if(s){{hv=true;vn++}}
    }});
    c.classList.toggle('hid',!hv);
    if(hv){{vm++;c.querySelector('.ch').classList.add('on');c.querySelector('.cb').classList.add('open')}}
  }});
  document.getElementById('rc-maps').textContent=vn>0?'找到 '+vn+' 个精灵（'+vm+' 个地图）':'无匹配结果'
}}

// ══════ 塔搜索 ══════
function tf(tag){{
  var q=(document.getElementById('si-'+tag).value||'').toLowerCase();
  var vf=0;
  document.querySelectorAll('#sec-'+tag+' .tower-card').forEach(function(c){{
    var t=c.textContent.toLowerCase().includes(q);
    c.classList.toggle('hid',!t);
    if(t){{vf++;c.querySelector('.ch').classList.add('on');c.querySelector('.cb').classList.add('open')}}
  }});
  document.getElementById('rc-'+tag).textContent=vf>0?'找到 '+vf+' 层':'无匹配结果'
}}

// ══════ 融合搜索 ══════
function tff(){{
  var q=(document.getElementById('si-fuse').value||'').toLowerCase();
  var vf=0;
  document.querySelectorAll('#sec-fuse .card').forEach(function(c){{
    var t=c.textContent.toLowerCase().includes(q);
    c.classList.toggle('hid',!t);
    if(t){{vf++;c.querySelector('.ch').classList.add('on');c.querySelector('.cb').classList.add('open')}}
  }});
  document.getElementById('rc-fuse').textContent=vf>0?'找到 '+vf+' 条':'无匹配结果'
}}

// ══════ 回到顶部 ══════
addEventListener('scroll',function(){{document.getElementById('btt').classList.toggle('show',scrollY>400)}})

// ══════ 点击空白关闭侧边栏 (移动端) ══════
document.addEventListener('click',function(e){{
  var sb=document.getElementById('sb');
  if(window.innerWidth<=900&&!sb.contains(e.target)&&e.target!==document.getElementById('sbt')){{
    sb.classList.remove('open');
  }}
}})
</script>
</body>
</html>"""


# ═══════════════════════════════════════════════
# 主流程
# ═══════════════════════════════════════════════

def main():
    print("=" * 50)
    print("  重启号 数据总览 → HTML 生成器")
    print("=" * 50)

    print("\n[1/7] 加载技能 & Buff ...")
    skills = load_skills()
    buffs = load_buffs()
    print(f"  技能 {len(skills)}  ·  Buff {len(buffs)}")

    print("\n[2/7] 加载精灵数据 ...")
    monsters = load_monsters()
    print(f"  精灵 {len(monsters)}")

    print("\n[3/7] 加载道具数据 ...")
    items = load_items()
    print(f"  道具 {len(items)}")

    print("\n[4/7] 解析地图 XML ...")
    xml_dir = os.path.join(BASE, "maps")
    xml_files = sorted(f for f in os.listdir(xml_dir) if f.endswith(".xml"))
    maps = []
    for f in xml_files:
        md = parse_map(os.path.join(xml_dir, f))
        if md: maps.append(md)
    maps.sort(key=lambda m: int(m["id"]) if m["id"].lstrip("-").isdigit() else 0, reverse=True)

    tb = sum(1 for m in maps for n in m["npcs"] if n["boss"])
    tr = sum(1 for m in maps for n in m["npcs"] if n["rare"])
    tn = sum(1 for m in maps for n in m["npcs"] if not n["boss"] and not n["rare"])
    print(f"  有效地图 {len(maps)}  ·  精灵 {tb+tr+tn} (BOSS:{tb} 稀有:{tr} 普通:{tn})")

    print("\n[5/7] 解析塔数据 ...")
    btp = os.path.join(BASE, "towers", "brave_tower.json")
    ttp = os.path.join(BASE, "towers", "trial_tower.json")
    brave_floors = process_tower(btp, skills, monsters, items) if os.path.exists(btp) else []
    trial_floors = process_tower(ttp, skills, monsters, items) if os.path.exists(ttp) else []
    print(f"  勇者之塔 {len(brave_floors)} 层  ·  试炼之塔 {len(trial_floors)} 层")

    print("\n[6/7] 解析融合数据 ...")
    new_se = load_new_se()
    formulas = parse_fusion_formula(items, new_se)
    recipes = parse_fusion_recipes(monsters, items)
    print(f"  特性 {len(new_se)}  ·  材料配方 {len(formulas)} 组  ·  融合配方 {len(recipes)} 个")

    print("\n[7/7] 生成 index.html ...")
    now = datetime.now().strftime("%Y-%m-%d %H:%M")
    html = gen_html(maps, skills, buffs, brave_floors, trial_floors, formulas, recipes, monsters, items, now)
    out = os.path.join(BASE, "index.html")
    with open(out, "w", encoding="utf-8") as f:
        f.write(html)
    size_kb = os.path.getsize(out) / 1024
    print(f"  输出: index.html  ({size_kb:.0f} KB)")
    print("\n  完成！双击 index.html 即可查看。\n")


if __name__ == "__main__":
    main()
