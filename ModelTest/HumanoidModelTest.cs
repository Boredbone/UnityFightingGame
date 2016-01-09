using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelTest
{
    [TestClass]
    public class HumanoidModelTest
    {

        private Subject<bool> Frame { get; } = new Subject<bool>();
        
        private AppCore core = AppCore.GetEnvironment(null);

        private HumanoidModel model;

        private PlayerSettings[] players = new[]
        {
            new PlayerSettings()
            {
                CharacterType = CharacterType.Humanoid,
                InputType = InputType.Keyboard,
                InputIndex = 0,
                Team = 0,
                Mirror = false,
                ColorIndex = 0,
            },
            new PlayerSettings()
            {
                CharacterType = CharacterType.Humanoid,
                InputType = InputType.Keyboard,
                InputIndex = 1,
                Team = 1,
                Mirror = true,
                ColorIndex = 1,
            },
        };



        [TestMethod]
        public async Task SinglePunchTest()
        {
            this.model = new HumanoidModel();
            
            this.core.Initialize();

            foreach (var player in this.players)
            {

                model.Initialize(this.core, player);

                await this.UpdateAndWaitAsync(model, 10, 0);

                this.PressButtonAsync(this.core.KeyboardInput[player.InputIndex].Buttons[0], 1)
                    .FireAndForget();

                await this.UpdateAndWaitAsync(model, 1, 0);

                Assert.IsTrue(model.DesiredParameters.Punch);

                await this.UpdateAndWaitAsync(model, 1, 0);

                Assert.IsFalse(model.DesiredParameters.Punch);
                
            }
            
        }

        [TestMethod]
        public async Task RushPunchSuccessTest()
        {
            this.model = new HumanoidModel();

            this.core.Initialize();

            foreach (var player in this.players)
            {

                model.Initialize(this.core, player);
                this.ClearButton(this.core.KeyboardInput[player.InputIndex].Buttons[0]);

                await Task.Delay(400);

                await this.UpdateAndWaitAsync(model, 10, 0);
                

                this.UpdateAndWaitAsync(model, 100, 30).FireAndForget();

                this.RepeatButtonAsync(this.core.KeyboardInput[player.InputIndex].Buttons[0],
                        1, 100, 5)
                        .FireAndForget();

                await Task.Delay(100);

                //Assert.IsTrue(model.DesiredParameters.Punch);
                Assert.IsFalse(model.DesiredParameters.RushPunch);

                await Task.Delay(200);
                
                Assert.IsTrue(model.DesiredParameters.RushPunch);

                await Task.Delay(400);
                
                Assert.IsTrue(model.DesiredParameters.RushPunch);

                await Task.Delay(400);

                Assert.IsFalse(model.DesiredParameters.Punch);
                Assert.IsFalse(model.DesiredParameters.RushPunch);
            }
        }

        [TestMethod]
        public async Task RushPunchFailTest()
        {
            this.model = new HumanoidModel();

            this.core.Initialize();

            foreach (var player in this.players)
            {

                model.Initialize(this.core, player);
                this.ClearButton(this.core.KeyboardInput[player.InputIndex].Buttons[0]);

                await Task.Delay(400);

                await this.UpdateAndWaitAsync(model, 10, 0);


                this.UpdateAndWaitAsync(model, 100, 30).FireAndForget();

                this.RepeatButtonAsync(this.core.KeyboardInput[player.InputIndex].Buttons[0],
                        1, 350, 5)
                        .FireAndForget();

                await Task.Delay(100);

                Assert.IsFalse(model.DesiredParameters.RushPunch);

                await Task.Delay(200);

                Assert.IsFalse(model.DesiredParameters.RushPunch);

                await Task.Delay(200);

                Assert.IsFalse(model.DesiredParameters.Punch);
                Assert.IsFalse(model.DesiredParameters.RushPunch);
            }
        }

        private async Task RepeatButtonAsync(KeyStatus key, int frame,int interval,int count)
        {

            await this.PressButtonAsync(key, frame);

            for (int i = 1; i < count; i++)
            {
                await Task.Delay(interval);
                await this.PressButtonAsync(key, frame);
            }
        }


        private async Task UpdateAndWaitAsync(HumanoidModel model,int count,int interval)
        {
            for (int i = 0; i < count; i++)
            {
                model.Update(null);
                this.Frame.OnNext(true);
                if (interval > 0)
                {
                    await Task.Delay(interval);
                }
            }
        }


        private async Task PressButtonAsync(KeyStatus key, int frame)
        {
            key.IsPressed = true;
            key.IsDown = true;
            key.IsReleased = false;

            await this.Frame.Take(1);

            if (frame > 1)
            {
                key.IsPressed = false;
                key.IsDown = true;
                key.IsReleased = false;

                await this.Frame.Take(frame);
            }

            key.IsPressed = false;
            key.IsDown = false;
            key.IsReleased = true;

            await this.Frame.Take(1);

            key.IsReleased = false;


        }

        //private async Task KeepButtonAsync(KeyStatus key, int time)
        //{
        //    key.IsPressed = false;
        //    key.IsDown = true;
        //    await Task.Delay(time);
        //    key.IsDown = false;
        //    key.IsReleased = true;
        //}
        //private void ReleaseButton(KeyStatus key)
        //{
        //    key.IsPressed = false;
        //    key.IsDown = false;
        //    key.IsReleased = true;
        //}

        private void ClearButton(KeyStatus key)
        {
            key.IsPressed = false;
            key.IsDown = false;
            key.IsReleased = false;
        }
    }

    static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                throw x.Exception.InnerException;
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
