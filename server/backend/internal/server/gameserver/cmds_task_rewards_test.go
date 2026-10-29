package gameserver

import "testing"

func TestStoryTaskRewardsExpPool(t *testing.T) {
	pairs, ok := storyTaskRewards(89)
	if !ok || len(pairs) < 1 {
		t.Fatal(pairs)
	}
	found := false
	for _, p := range pairs {
		if p[0] == taskRewardExpPool && p[1] == 500 {
			found = true
		}
	}
	if !found {
		t.Fatalf("want exp 500: %v", pairs)
	}
}

func TestInstructorExamRewards(t *testing.T) {
	pairs, ok := storyTaskRewards(taskIDInstructorExam)
	if !ok || len(pairs) != 6 {
		t.Fatalf("want 6 suit items got ok=%v %v", ok, pairs)
	}
	want := []uint32{100062, 100068, 100069, 100070, 100071, 100072}
	for i, id := range want {
		if pairs[i][0] != id || pairs[i][1] != 1 {
			t.Fatalf("idx %d want %d×1 got %v", i, id, pairs[i])
		}
	}
}

func TestResolveDailyDefaultExp(t *testing.T) {
	pairs := resolveTaskRewards(9999, true)
	if len(pairs) != 1 || pairs[0][0] != taskRewardExpPool || pairs[0][1] != defaultTaskExpPool {
		t.Fatalf("%v", pairs)
	}
}

func TestNoviceTasksSkipDefault(t *testing.T) {
	for _, id := range []uint32{86, 87, 88} {
		pairs, ok := storyTaskRewards(id)
		if !ok {
			t.Fatalf("task %d should be specialized", id)
		}
		if len(pairs) != 0 {
			t.Fatalf("task %d should not use story pairs: %v", id, pairs)
		}
	}
}

func TestTrainingCampNeedsMonBall(t *testing.T) {
	pairs := resolveTaskRewards(401, true)
	if len(pairs) < 1 || pairs[0][0] != taskRewardExpPool {
		t.Fatalf("%v", pairs)
	}
}
