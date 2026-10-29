package gameserver

import "testing"

func TestSkillSlotsSameMultiset(t *testing.T) {
	if !skillSlotsSameMultiset([]int{10001, 10002, 20001, 0}, []int{20001, 10001, 0, 10002}) {
		t.Fatal("reorder should be same multiset")
	}
	if skillSlotsSameMultiset([]int{10001, 10002}, []int{10001, 10003}) {
		t.Fatal("different ids")
	}
	if skillSlotsSameMultiset([]int{10001, 10002, 0, 0}, []int{10001, 10002, 10003, 0}) {
		t.Fatal("extra skill")
	}
	if !skillSlotsSameMultiset([]int{10001, 10001, 0, 0}, []int{10001, 0, 10001, 0}) {
		t.Fatal("duplicate ids reorder")
	}
}

func TestHasNonoFuncBitSkillSortChip(t *testing.T) {
	bits := make([]byte, 20)
	if hasNonoFuncBit(bits, skillSortChipItemID) {
		t.Fatal("empty bits")
	}
	setNonoFuncBit(bits, skillSortChipItemID)
	if !hasNonoFuncBit(bits, skillSortChipItemID) {
		t.Fatal("bit 700016 not set")
	}
}
