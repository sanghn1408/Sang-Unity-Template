using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PoolManager;

public class PoolManager : Singleton<PoolManager>
{
    //[System.Serializable]
    //public class BulletPool
    //{
    //    public FireworkBullet _bullets;
    //    public List<FireworkBullet> inactiveObjs;
    //    public List<FireworkBullet> activeObjs;
    //    public FireworkBullet SpawnBullet(FireworkBullet bullet, Vector3 pos)
    //    {

    //        if (inactiveObjs.Count == 0)
    //        {
    //            FireworkBullet obj = Instantiate(_bullets);
    //            obj.transform.position = pos;
    //            activeObjs.Add(obj);
    //            return obj;
    //        }
    //        else
    //        {
    //            FireworkBullet obj = Instantiate(bullet);
    //            obj.transform.position = pos;
    //            activeObjs.Add(obj);
    //            return obj;
    //        }
    //    }
    //    public void Clear()
    //    {
    //        while (activeObjs.Count > 0)
    //        {
    //            FireworkBullet obj = activeObjs[0];
    //            inactiveObjs.Add(obj);
    //            activeObjs.RemoveAt(0);
    //            obj.gameObject.SetActive(false);
    //            Debug.Log("clear bullet");
    //        }
    //    }
    //}
    ////
    //public List<FireworkBullet> _bullets;
    //[SerializeField] private BulletPool _BulletPools;
    //FireworkBullet _bullet;
    //public void SpawnBullet(FireworkBullet obj, Vector3 Pos)
    //{
    //    _bullet = _BulletPools.SpawnBullet(obj,Pos);
    //}
    //public void ClearAllBullets()
    //{
    //    _BulletPools.Clear();
    //}
}
