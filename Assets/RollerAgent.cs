using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

// RollerAgent
public class RollerAgent : Agent
{
    Rigidbody rBody; // RollerAgentのRigidBody
    public Target target_obj;
    private float old_distanceToTarget;
    Vector3 old_targetPos;

    // 初期化時に呼ばれる
    public override void Initialize()
    {
        // RollerAgentのRigidBodyの参照の取得
        this.rBody = GetComponent<Rigidbody>();
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // RollerAgentが床から落下している時
        if (this.transform.localPosition.y < 0)
        {
            // RollerAgentの位置と速度をリセット
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        }

        // Targetの位置・角速度のリセット
        target_obj.setRandomPosition();
        target_obj.setRandomOmega();

        old_distanceToTarget = Vector3.Distance(
            this.transform.localPosition, target_obj.transform.position);
        this.old_targetPos = target_obj.transform.position;
    }

    // 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target_obj.transform.position.x); //TargetのX座標
        sensor.AddObservation(target_obj.transform.position.z); //TargetのZ座標
        sensor.AddObservation(old_targetPos.x); //過去のTargetのX座標
        sensor.AddObservation(old_targetPos.z); //過去のTargetのZ座標
        sensor.AddObservation(this.transform.localPosition.x); //RollerAgentのX座標
        sensor.AddObservation(this.transform.localPosition.z); //RollerAgentのZ座標
        sensor.AddObservation(rBody.velocity.x); // RollerAgentのX速度
        sensor.AddObservation(rBody.velocity.z); // RollerAgentのZ速度
        old_targetPos = target_obj.transform.position;
    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // RollerAgentに力を加える
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * 10);

        // RollerAgentがTargetの位置にたどりついた時
        float distanceToTarget = Vector3.Distance(
            this.transform.localPosition, target_obj.transform.position);
        // AddReward(-distanceToTarget * (float)0.01);

        // 遠ざかったサブリワード
        if (distanceToTarget > old_distanceToTarget) {
            AddReward(-0.01f);
        }
        old_distanceToTarget = distanceToTarget;

        if (distanceToTarget < 1.42f)
        {
            AddReward(2.0f);
            EndEpisode();
        }

        // RollerAgentが床から落下した時
        if (this.transform.localPosition.y < 0)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}