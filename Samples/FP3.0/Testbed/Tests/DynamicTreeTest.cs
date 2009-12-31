﻿/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class DynamicTreeTest : Test
    {
        private const int ActorCount = 128;

        private DynamicTreeTest()
        {
            _worldExtent = 15.0f;
            _proxyExtent = 0.5f;

            for (int i = 0; i < ActorCount; ++i)
            {
                _actors[i] = new Actor();

                Actor actor = _actors[i];
                GetRandomAABB(out actor.aabb);
                actor.proxyId = _tree.CreateProxy(ref actor.aabb, actor);
            }

            _stepCount = 0;

            float h = _worldExtent;
            _queryAABB.LowerBound = new Vector2(-3.0f, -4.0f + h);
            _queryAABB.UpperBound = new Vector2(5.0f, 6.0f + h);

            _rayCastInput.Point1 = new Vector2(-5.0f, 5.0f + h);
            _rayCastInput.Point2 = new Vector2(7.0f, -4.0f + h);
            //_rayCastInput.p1 = new Vector2(0.0f, 2.0f + h);
            //_rayCastInput.p2 = new Vector2(0.0f, -2.0f + h);
            _rayCastInput.MaxFraction = 1.0f;

            _automated = false;
        }

        internal static Test Create()
        {
            return new DynamicTreeTest();
        }

        public override void Step(Framework.Settings settings)
        {
            _rayActor = null;
            for (int i = 0; i < ActorCount; ++i)
            {
                _actors[i].fraction = 1.0f;
                _actors[i].overlap = false;
            }

            if (_automated)
            {
                int actionCount = Math.Max(1, ActorCount >> 2);

                for (int i = 0; i < actionCount; ++i)
                {
                    Action();
                }
            }

            Query();
            RayCast();

            for (int i = 0; i < ActorCount; ++i)
            {
                Actor actor = _actors[i];
                if (actor.proxyId == -1)
                    continue;

                Color ca = new Color(0.9f, 0.9f, 0.9f);
                if (actor == _rayActor && actor.overlap)
                {
                    ca = new Color(0.9f, 0.6f, 0.6f);
                }
                else if (actor == _rayActor)
                {
                    ca = new Color(0.6f, 0.9f, 0.6f);
                }
                else if (actor.overlap)
                {
                    ca = new Color(0.6f, 0.6f, 0.9f);
                }

                _debugView.DrawAABB(ref actor.aabb, ca);
            }

            Color c = new Color(0.7f, 0.7f, 0.7f);
            _debugView.DrawAABB(ref _queryAABB, c);

            _debugView.DrawSegment(_rayCastInput.Point1, _rayCastInput.Point2, c);

            Color c1 = new Color(0.2f, 0.9f, 0.2f);
            Color c2 = new Color(0.9f, 0.2f, 0.2f);
            _debugView.DrawPoint(_rayCastInput.Point1, 6.0f, c1);
            _debugView.DrawPoint(_rayCastInput.Point2, 6.0f, c2);

            if (_rayActor != null)
            {
                Color cr = new Color(0.2f, 0.2f, 0.9f);
                Vector2 p = _rayCastInput.Point1 + _rayActor.fraction * (_rayCastInput.Point2 - _rayCastInput.Point1);
                _debugView.DrawPoint(p, 6.0f, cr);
            }

            ++_stepCount;
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
            {
                _automated = !_automated;
            }
            if (state.IsKeyDown(Keys.C) && oldState.IsKeyUp(Keys.C))
            {
                CreateProxy();
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                DestroyProxy();
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                MoveProxy();
            }
        }

        private bool QueryCallback(int proxyid)
        {
            Actor actor = (Actor)_tree.GetUserData(proxyid);
            actor.overlap = AABB.TestOverlap(ref _queryAABB, ref actor.aabb);
            return true;
        }

        private float RayCastCallback(ref RayCastInput input, int proxyid)
        {
            Actor actor = (Actor)_tree.GetUserData(proxyid);

            RayCastOutput output;
            bool hit = actor.aabb.RayCast(out output, ref input);

            if (hit)
            {
                actor.fraction = output.Fraction;
                _rayActor = actor;

                return output.Fraction;
            }

            return input.MaxFraction;
        }

        private class Actor
        {
            internal AABB aabb;
            internal float fraction;
            internal bool overlap;
            internal int proxyId;
        } ;

        private void GetRandomAABB(out AABB aabb)
        {
            aabb = new AABB();

            Vector2 w = new Vector2(2.0f * _proxyExtent, 2.0f * _proxyExtent);
            //aabb.lowerBound.x = -_proxyExtent;
            //aabb.lowerBound.y = -_proxyExtent + _worldExtent;
            aabb.LowerBound.X = Rand.RandomFloat(-_worldExtent, _worldExtent);
            aabb.LowerBound.Y = Rand.RandomFloat(0.0f, 2.0f * _worldExtent);
            aabb.UpperBound = aabb.LowerBound + w;
        }

        private void MoveAABB(ref AABB aabb)
        {
            Vector2 d = Vector2.Zero;
            d.X = Rand.RandomFloat(-0.5f, 0.5f);
            d.Y = Rand.RandomFloat(-0.5f, 0.5f);
            //d.x = 2.0f;
            //d.y = 0.0f;
            aabb.LowerBound += d;
            aabb.UpperBound += d;

            Vector2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
            Vector2 min = new Vector2(-_worldExtent, 0.0f);
            Vector2 max = new Vector2(_worldExtent, 2.0f * _worldExtent);
            Vector2 c = Vector2.Clamp(c0, min, max);

            aabb.LowerBound += c - c0;
            aabb.UpperBound += c - c0;
        }

        private void CreateProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.rand.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == -1)
                {
                    GetRandomAABB(out actor.aabb);
                    actor.proxyId = _tree.CreateProxy(ref actor.aabb, actor);
                    return;
                }
            }
        }

        private void DestroyProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.rand.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.proxyId != -1)
                {
                    _tree.DestroyProxy(actor.proxyId);
                    actor.proxyId = -1;
                    return;
                }
            }
        }

        private void MoveProxy()
        {
            for (int i = 0; i < ActorCount; ++i)
            {
                int j = Rand.rand.Next() % ActorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == -1)
                {
                    continue;
                }

                AABB aabb0 = actor.aabb;
                MoveAABB(ref actor.aabb);
                Vector2 displacement = actor.aabb.GetCenter() - aabb0.GetCenter();
                _tree.MoveProxy(actor.proxyId, ref actor.aabb, displacement);
                return;
            }
        }

        private void Action()
        {
            int choice = Rand.rand.Next() % 20;

            switch (choice)
            {
                case 0:
                    CreateProxy();
                    break;

                case 1:
                    DestroyProxy();
                    break;

                default:
                    MoveProxy();
                    break;
            }
        }

        private void Query()
        {
            _tree.Query(QueryCallback, ref _queryAABB);

            for (int i = 0; i < ActorCount; ++i)
            {
                if (_actors[i].proxyId == -1)
                {
                    continue;
                }

                bool overlap = AABB.TestOverlap(ref _queryAABB, ref _actors[i].aabb);
                Debug.Assert(overlap == _actors[i].overlap);
            }
        }

        private void RayCast()
        {
            _rayActor = null;

            RayCastInput input = _rayCastInput;

            // Ray cast against the dynamic tree.
            _tree.RayCast(RayCastCallback, ref input);

            // Brute force ray cast.
            for (int i = 0; i < ActorCount; ++i)
            {
                if (_actors[i].proxyId == -1)
                {
                    continue;
                }

                RayCastOutput output;
                bool hit = _actors[i].aabb.RayCast(out output, ref input);
                if (hit)
                {
                    input.MaxFraction = output.Fraction;
                }
            }
        }

        private float _worldExtent;
        private float _proxyExtent;

        private DynamicTree _tree = new DynamicTree();
        private AABB _queryAABB;
        private RayCastInput _rayCastInput;
        private Actor _rayActor = new Actor();
        private Actor[] _actors = new Actor[ActorCount];
        private int _stepCount;
        private bool _automated;
    }
}